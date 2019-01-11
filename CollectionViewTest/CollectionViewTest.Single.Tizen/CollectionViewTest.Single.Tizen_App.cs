using Tizen.Applications;
using ElmSharp;
using Xamarin.Forms.Platform.Tizen.Native;
using System.Collections.Generic;

namespace CollectionViewTest.Single.Tizen
{
    class App : CoreUIApplication
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            // Initialize();
			test();
        }

		void test()
		{

			Window window = new Window("ElmSharpApp")
			{
				AvailableRotations = DisplayRotation.Degree_0 | DisplayRotation.Degree_180 | DisplayRotation.Degree_270 | DisplayRotation.Degree_90
			};
			window.BackButtonPressed += (s, e) =>
			{
				Exit();
			};
			window.Show();


			var cv = new CollectionView(window);
			//var manager = new LinearLayoutManager(false);
			//manager.CollectionView = cv;
			//cv.LayoutManager = manager;
			//var item = new ElmSharp.Label(window)
			//{
			//	Text = "label",
			//	Color = Color.Black
			//};
			//var list = new List<ElmSharp.Label>();
			//list.Add(item);
			//var adaptor = new EmptyItemAdaptor(null, list, null);
			cv.Show();

			var bg = new Background(window)
			{
				Color = Color.Purple
			};
			bg.SetContent(cv);

			var conformant = new Conformant(window);
			conformant.Show();
			conformant.SetContent(bg);

		}

		void Initialize()
        {
            Window window = new Window("ElmSharpApp")
            {
                AvailableRotations = DisplayRotation.Degree_0 | DisplayRotation.Degree_180 | DisplayRotation.Degree_270 | DisplayRotation.Degree_90
            };
            window.BackButtonPressed += (s, e) =>
            {
                Exit();
            };
            window.Show();

			var box = new ElmSharp.Box(window)
            {
                AlignmentX = -1,
                AlignmentY = -1,
                WeightX = 1,
                WeightY = 1,
            };
            box.Show();

            var bg = new Background(window)
            {
                Color = Color.White
            };
            bg.SetContent(box);

            var conformant = new Conformant(window);
            conformant.Show();
            conformant.SetContent(bg);

            var label = new ElmSharp.Label(window)
            {
                Text = "Hello, Tizen",
                Color = Color.Black
            };
            label.Show();
            box.PackEnd(label);
        }

        static void Main(string[] args)
        {
            Elementary.Initialize();
            Elementary.ThemeOverlay();
            App app = new App();
            app.Run(args);
        }
    }
}
