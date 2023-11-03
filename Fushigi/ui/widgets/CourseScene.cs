﻿using Fushigi.Byml;
using Fushigi.Byml.Writer;
using Fushigi.course;
using Fushigi.param;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using Silk.NET.Windowing;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Linq;

namespace Fushigi.ui.widgets
{
    class CourseScene
    {

        LevelViewport viewport;
        readonly Course course;
        CourseArea selectedArea;

        readonly Dictionary<string, bool> mLayersVisibility = [];
        bool mHasFilledLayers = false;
        bool mAllLayersVisible = true;
        bool mShowAddActor = false;

        BymlHashTable? mSelectedActor = null;

        readonly Dictionary<string, bool> mCategoryVisibility = [];

        Dictionary<string, List<BymlHashTable>> ActorCategories = new Dictionary<string, List<BymlHashTable>>();

        public CourseScene(Course course)
        {
            this.course = course;
            selectedArea = course.GetArea(0);
            viewport = new LevelViewport(selectedArea);
            ReloadArea();
        }

        public void DrawUI(GL gl)
        {
            bool status = ImGui.Begin("Course");

            CourseTabBar();

            viewport.Draw(gl, ImGui.GetContentRegionAvail(), mLayersVisibility, 
                selectedActors: new HashSet<BymlHashTable>()); //only temporary

            AreaParameterPanel();

            ActorsPanel(gl);

            ActorParameterPanel();

            if (mShowAddActor)
            {
                SelectActor();
            }

            if (viewport.HasSelectionChanged())
            {
                BymlHashTable selectedActor = viewport.GetSelectedActors().ElementAt(0);
                mSelectedActor = selectedActor;
            }

            if (status)
            {
                ImGui.End();
            }
        }

        public void Save()
        {
            //Save each course area to current romfs folder
            foreach (var area in this.course.GetAreas())
                area.Save();
        }

        public void Save(string folder)
        {
            //Save each course area to a specific folder
            foreach (var area in this.course.GetAreas())
                area.Save(folder);
        }

        private void CourseTabBar()
        {
            bool tabStatus = ImGui.BeginTabBar("Courses TabBar"); // Not sure what the string argument is for

            foreach (var area in course.GetAreas())
            {
                if (ImGui.BeginTabItem(area.GetName()))
                {
                    // Tab change
                    if (selectedArea != area)
                    {
                        selectedArea = area;
                        viewport = new(area);

                        // Unselect actor
                        // This is so that users do not see an actor selected from another area
                        mSelectedActor = null;
                        ReloadArea();
                    }

                    ImGui.EndTabItem();
                }
            }

            if (tabStatus)
            {
                ImGui.EndTabBar();
            }
        }

        private void ReloadArea()
        {
            if (ActorDB.Actors.Count == 0)
                ActorDB.Init();

            ActorCategories.Clear();

            var root = selectedArea.GetRootNode();
            // actors are in an array
            BymlArrayNode actorArray = (BymlArrayNode)((BymlHashTable)root)["Actors"];

            mCategoryVisibility.Clear();
            foreach (BymlHashTable node in actorArray.Array.Cast<BymlHashTable>())
            {
                string actorName = ((BymlNode<string>)node["Gyaml"]).Data;
                var actor = ActorDB.Actors[actorName];
                if (!this.ActorCategories.ContainsKey(actor.Category))
                {
                    this.ActorCategories.Add(actor.Category, new List<BymlHashTable>());
                    mCategoryVisibility.Add(actor.Category, true);
                }

                ActorCategories[actor.Category].Add(node);
            }
        }

        private void SelectActor()
        {
            bool status = ImGui.Begin("Add Actor");

            ImGui.BeginListBox("Select the actor you want to add.", ImGui.GetContentRegionAvail());

            foreach (string actor in ParamDB.GetActors())
            {
                ImGui.Selectable(actor);

                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0))
                {
                    viewport.mEditorState = LevelViewport.EditorState.AddingActor;
                    viewport.mActorToAdd = actor;
                    mShowAddActor = false;
                }
            }

            ImGui.EndListBox();

            if (status)
            {
                ImGui.End();
            }
        }

        private void ActorsPanel(GL gl)
        {
            var root = selectedArea.GetRootNode();

            ImGui.Begin("Actors");

            if (ImGui.Button("Add Actor"))
            {
                mShowAddActor = true;
            }

            if (ImGui.Button("Delete Actor"))
            {
                viewport.mEditorState = LevelViewport.EditorState.DeletingActor;
            }

            // actors are in an array
            BymlArrayNode actorArray = (BymlArrayNode)((BymlHashTable)root)["Actors"];

            ImGui.BeginTabBar("actor_tab");

            if (ImGui.BeginTabItem("Layers"))
            {
                CourseActorsLayerView(actorArray);
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Tree"))
            {
                CourseActorsTreeView(actorArray, gl);
                ImGui.EndTabItem();
            }

            ImGui.End();
        }

        private void ActorParameterPanel()
        {
            bool status = ImGui.Begin("Actor Parameters");

            if (mSelectedActor is null)
            {
                ImGui.Text("No Actor is selected");
            }
            else
            {
                string actorName = ((BymlNode<string>)mSelectedActor["Gyaml"]).Data;
                string name = ((BymlNode<string>)mSelectedActor["Name"]).Data;

                ImGui.Text(actorName);

                ImGui.Separator();

                ImGui.Columns(2);

                ImGui.Text("Name");

                ImGui.NextColumn();
                ImGui.PushItemWidth(ImGui.GetColumnWidth() - 12);
                ImGui.InputText($"##{name}", ref name, 512);

                ImGui.Columns(1);

                if (ImGui.BeginChild("Placement"))
                {
                    PlacementNode(mSelectedActor);
                }

                /* actor parameters are loaded from the dynamic node */
                if (mSelectedActor.ContainsKey("Dynamic"))
                {
                    DynamicParamNode(mSelectedActor, actorName);
                }
            }
            

            if (status)
            {
                ImGui.End();
            }
        }

        private void AreaParameterPanel()
        {
            bool status = ImGui.Begin("Course Area Parameters");

            ImGui.Text(selectedArea.GetName());

            AreaParameters(selectedArea.mAreaParams);

            if (status)
            {
                ImGui.End();
            }
        }

        private static void AreaParameters(CourseArea.AreaParam area)
        {
            ParamHolder areaParams = ParamLoader.GetHolder("AreaParam");

            foreach (string key in areaParams.Keys)
            {
                string paramType = areaParams[key];

                if (!area.ContainsParam(key))
                {
                    continue;
                }

                switch (paramType)
                {
                    case "String":
                        string value = (string)area.GetParam(area.GetRoot(), key, paramType);
                        ImGui.InputText(key, ref value, 1024);

                        break;
                }
            }
        }

        private void FillLayers(BymlArrayNode actorArray)
        {
            foreach (BymlHashTable node in actorArray.Array.Cast<BymlHashTable>())
            {
                string actorLayer = ((BymlNode<string>)node["Layer"]).Data;
                mLayersVisibility[actorLayer] = true;
            }
            mHasFilledLayers = true;
        }

        private void CourseActorsTreeView(BymlArrayNode actorArray, GL gl)
        {
            foreach (var category in ActorCategories)
            {
                bool vis = mCategoryVisibility[category.Key];
                if (ImGui.Checkbox($"##{category}chk", ref vis))
                {
                    mCategoryVisibility[category.Key] = vis;
                }
                ImGui.SameLine();

                if (ImGui.TreeNode(category.Key))
                {
                    ImGui.Indent();

                    foreach (BymlHashTable node in category.Value)
                    {
                        ulong actorHash = ((BymlBigDataNode<ulong>)node["Hash"]).Data;
                        string actorName = ((BymlNode<string>)node["Gyaml"]).Data;
                        string name = ((BymlNode<string>)node["Name"]).Data;

                        var actorResDB = ActorDB.Actors[actorName];
                        var iconID = actorResDB.GetIcon(gl);
                        if (iconID == -1)
                        {

                        }
                        /*   string actorName = ((BymlNode<string>)node["Gyaml"]).Data;
                           if (ImGui.TreeNodeEx(actorName, ImGuiTreeNodeFlags.Leaf))
                           {
                               ImGui.TreePop();
                           }*/

                        bool isSelected = (node == mSelectedActor);

                        ImGui.PushID(actorHash.ToString());
                        ImGui.Columns(2);


                        bool avis = true;
                        if (ImGui.Checkbox($"##{actorHash}vis", ref avis))
                        {
                        }
                        ImGui.SameLine();

                        ImGui.Image(iconID, new Vector2(22, 22));
                        ImGui.SameLine();

                        if (ImGui.Selectable(actorName, isSelected, ImGuiSelectableFlags.SpanAllColumns))
                        {
                            mSelectedActor = node;
                        }
                        ImGui.NextColumn();
                        ImGui.BeginDisabled();
                        ImGui.Text(name);
                        ImGui.EndDisabled();
                        ImGui.Columns(1);
                    }
                    ImGui.Unindent();

                    ImGui.TreePop();
                }
            }
        }

        private void CourseActorsLayerView(BymlArrayNode actorArray)
        {
            if (!mHasFilledLayers)
            {
                FillLayers(actorArray);
            }

            if (ImGui.Checkbox("All Layers", ref mAllLayersVisible))
            {
                foreach (string layer in mLayersVisibility.Keys)
                {
                    mLayersVisibility[layer] = mAllLayersVisible;
                }
            }

            foreach (string layer in mLayersVisibility.Keys)
            {
                bool isVisible = mLayersVisibility[layer];
                if (ImGui.Checkbox("##" + layer, ref isVisible))
                {
                    mLayersVisibility[layer] = isVisible;
                }

                ImGui.SameLine();

                if (!isVisible)
                {
                    ImGui.BeginDisabled();
                }

                if (ImGui.CollapsingHeader(layer, ImGuiTreeNodeFlags.Selected))
                {
                    ImGui.Indent();
                    ImGui.PushItemWidth(ImGui.GetColumnWidth());
                    if (ImGui.BeginListBox("##" + layer))
                    {
                        foreach (BymlHashTable node in actorArray.Array.Cast<BymlHashTable>())
                        {
                            string actorName = ((BymlNode<string>)node["Gyaml"]).Data;
                            string name = ((BymlNode<string>)node["Name"]).Data;
                            ulong actorHash = ((BymlBigDataNode<ulong>)node["Hash"]).Data;
                            string actorLayer = ((BymlNode<string>)node["Layer"]).Data;

                            if (actorLayer != layer)
                            {
                                continue;
                            }

                            bool isSelected = (node == mSelectedActor);

                            ImGui.PushID(actorHash.ToString());
                            ImGui.Columns(2);
                            if (ImGui.Selectable(actorName, isSelected, ImGuiSelectableFlags.SpanAllColumns))
                            {
                                mSelectedActor = node;
                            }
                            ImGui.NextColumn();
                            ImGui.BeginDisabled();
                            ImGui.Text(name);
                            ImGui.EndDisabled();
                            ImGui.Columns(1);

                            ImGui.PopID();
                        }
                        ImGui.EndListBox();
                    }
                    ImGui.Unindent();
                }

                if (!isVisible)
                {
                    ImGui.EndDisabled();
                }
            }
        }

        private static void PlacementNode(BymlHashTable node)
        {
            void EditFloat3(string label, BymlArrayNode node)
            {
                var vec = new System.Numerics.Vector3(
                           ((BymlNode<float>)node[0]).Data,
                           ((BymlNode<float>)node[1]).Data,
                           ((BymlNode<float>)node[2]).Data);

                ImGui.Text(label);
                ImGui.NextColumn();

                ImGui.PushItemWidth(ImGui.GetColumnWidth() - 12);

                if (ImGui.DragFloat3($"##{label}", ref vec))
                {
                    ((BymlNode<float>)node[0]).Data = vec.X;
                    ((BymlNode<float>)node[1]).Data = vec.Y;
                    ((BymlNode<float>)node[2]).Data = vec.Z;
                }
                ImGui.PopItemWidth();

                ImGui.NextColumn();
            }

            if (ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent();
                ImGui.Columns(2);

                EditFloat3("Scale", (BymlArrayNode)node["Scale"]);
                EditFloat3("Rotation", (BymlArrayNode)node["Rotate"]);
                EditFloat3("Position", (BymlArrayNode)node["Translate"]);

                ImGui.Columns(1);
                ImGui.Unindent();
            }
        }

        private void DynamicParamNode(BymlHashTable node, string actorName)
        {
            List<string> actorParams = ParamDB.GetActorComponents(actorName);
            var dynamicNode = (BymlHashTable)node["Dynamic"];

            foreach (string param in actorParams)
            {
                Dictionary<string, ParamDB.ComponentParam> dict = ParamDB.GetComponentParams(param);

                if (dict.Keys.Count == 0)
                {
                    continue;
                }

                if (ImGui.CollapsingHeader(param))
                {
                    ImGui.Indent();

                    ImGui.Columns(2);

                    foreach (KeyValuePair<string, ParamDB.ComponentParam> pair in ParamDB.GetComponentParams(param))
                    {
                        string id = $"##{pair.Key}";

                        ImGui.Text(pair.Key);
                        ImGui.NextColumn();

                        ImGui.PushItemWidth(ImGui.GetColumnWidth() - 12);

                        if (dynamicNode.ContainsKey(pair.Key))
                        {
                            var paramNode = dynamicNode[pair.Key];

                            switch (pair.Value.Type)
                            {
                                case "S16":
                                case "S32":
                                    ImGui.InputInt(id, ref ((BymlNode<int>)paramNode).Data);
                                    break;
                                case "Bool":
                                    ImGui.Checkbox(id, ref ((BymlNode<bool>)paramNode).Data);
                                    break;
                                case "F32":
                                    ImGui.InputFloat(id, ref ((BymlNode<float>)paramNode).Data);
                                    break;
                                case "String":
                                    ImGui.InputText(id, ref ((BymlNode<string>)paramNode).Data, 1024);
                                    break;
                                case "F64":
                                    double val = ((BymlBigDataNode<double>)paramNode).Data;
                                    ImGui.InputDouble(id, ref val);
                                    break;
                            }
                        }
                        else
                        {
                            //object initValue = ;
                            switch (pair.Value.Type)
                            {
                                case "S16":
                                case "S32":
                                case "U32":
                                    {
                                        int val = Convert.ToInt32(pair.Value.InitValue);
                                        ImGui.InputInt(id, ref val);
                                        break;
                                    }

                                case "Bool":
                                    {
                                        bool val = (bool)pair.Value.InitValue;
                                        ImGui.Checkbox(id, ref val);
                                        break;
                                    }
                                case "F32":
                                    {
                                        float val = Convert.ToSingle(pair.Value.InitValue);
                                        ImGui.InputFloat(id, ref val);
                                        break;
                                    }

                                case "F64":
                                    {
                                        double val = Convert.ToDouble(pair.Value.InitValue);
                                        if (ImGui.InputDouble(id, ref val))
                                        {

                                        }
                                        break;
                                    }
                            }
                        }

                        ImGui.PopItemWidth();

                        ImGui.NextColumn();
                    }

                    ImGui.Columns(1);

                    ImGui.Unindent();
                }
            }
        }

        public Course GetCourse()
        {
            return course;
        }
    }
}
