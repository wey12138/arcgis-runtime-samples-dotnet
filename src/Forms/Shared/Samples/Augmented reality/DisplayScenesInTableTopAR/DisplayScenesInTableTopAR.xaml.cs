﻿// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Xamarin.Forms;
using System.Linq;
using ArcGISRuntime.Samples.Managers;

namespace ArcGISRuntimeXamarin.Samples.DisplayScenesInTabletopAR
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display scenes in tabletop AR",
        "Augmented reality",
        "Use augmented reality (AR) to pin a scene to a table or desk for easy exploration.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("7dd2f97bb007466ea939160d0de96a9d")]
    public partial class DisplayScenesInTabletopAR : ContentPage
    {
        // Scene to be displayed on the tabletop.
        private Scene _tabletopScene;

        public DisplayScenesInTabletopAR()
        {
            InitializeComponent();
            Initialize();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await MyARSceneView.StartTrackingAsync();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await MyARSceneView.StopTrackingAsync();
        }

        private void Initialize()
        {
            // Display an empty scene to enable tap-to-place.
            MyARSceneView.Scene = new Scene(SceneViewTilingScheme.Geographic);

            // Render the scene invisible to start.
            MyARSceneView.Scene.BaseSurface.Opacity = 0;

            // Get notification when planes are detected
            MyARSceneView.PlanesDetectedChanged += ARSceneView_PlanesDetectedChanged;
        }

        private void ARSceneView_PlanesDetectedChanged(object sender, bool planeDetected)
        {
            if (planeDetected)
            {
                EnableTapToPlace();
                MyARSceneView.PlanesDetectedChanged -= ARSceneView_PlanesDetectedChanged;
            }
        }

        private void EnableTapToPlace()
        {
            // Show the help label.
            //_helpLabel.Hidden = false;
            //_helpLabel.Text = "Tap to place the scene.";

            // Wait for the user to tap.
            MyARSceneView.GeoViewTapped += ARGeoViewTapped;
        }

        private void ARGeoViewTapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
            if (MyARSceneView.SetInitialTransformation(e.Position))
            {
                DisplayScene();
                //_arKitStatusLabel.Hidden = true;
            }
        }

        private async void DisplayScene()
        {
            // Hide the help label.
            //_helpLabel.Hidden = true;

            if (_tabletopScene == null)
            {
                // Get the downloaded mobile scene package.
                MobileScenePackage package =
                    await MobileScenePackage.OpenAsync(DataManager.GetDataFolder("7dd2f97bb007466ea939160d0de96a9d",
                        "philadelphia.mspk"));

                // Load the package.
                await package.LoadAsync();

                // Get the first scene.
                _tabletopScene = package.Scenes.First();

                // Hide the base surface.
                _tabletopScene.BaseSurface.Opacity = 0;

                // Enable subsurface navigation. This allows you to look at the scene from below.
                _tabletopScene.BaseSurface.NavigationConstraint = NavigationConstraint.None;

                MyARSceneView.Scene = _tabletopScene;

            }

            // Create a camera at the bottom and center of the scene.
            //    This camera is the point at which the scene is pinned to the real-world surface.
            Camera originCamera = new Camera(39.95787000283599,
                -75.16996728256345,
                8.813445091247559,
                0, 90, 0);

            // Set the origin camera.
            MyARSceneView.OriginCamera = originCamera;

            // The width of the scene content is about 800 meters.
            double geographicContentWidth = 800;

            // The desired physical width of the scene is 1 meter.
            double tableContainerWidth = 1;

            // Set the translation factor based on the scene content width and desired physical size.
            MyARSceneView.TranslationFactor = geographicContentWidth / tableContainerWidth;
        }

    }
}