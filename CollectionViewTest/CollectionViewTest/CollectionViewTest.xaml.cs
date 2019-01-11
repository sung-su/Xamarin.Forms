using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace CollectionViewTest
{
	public class TestItem
	{
		public string TestString1 { get; set; }

		public string TestString2 { get; set; }

		public TestItem(string testStr)
		{
			TestString1 = testStr;
			TestString2 = testStr;
		}
	}

	public partial class CollectionViewTest : Application
	{
		public CollectionViewTest()
		{
			// MainPage = new NavigationPage(new CollectionViewGallery());
			test();
		}


		void test()
		{
			var cvRed = new CollectionView
			{
				HeightRequest = 301,
				BackgroundColor = Color.Red,
			};

			var cvGreen = new CollectionView
			{
				HeightRequest = 302,
				BackgroundColor = Color.Green,
			};

			var cvBlue = new CollectionView
			{
				HeightRequest = 303,
				BackgroundColor = Color.Blue,
			};

			var verticalList = new ListItemsLayout(ItemsLayoutOrientation.Vertical);
			var horizontallList = new ListItemsLayout(ItemsLayoutOrientation.Horizontal);

			var verticalGrid = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical);
			var horizontalGrid = new GridItemsLayout(2, ItemsLayoutOrientation.Horizontal);

			var template1 = new DataTemplate(() =>
			{
				var label = new Label
				{
					HorizontalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Center,
					WidthRequest = 50,
					HeightRequest = 50,
					BackgroundColor = Color.Black,
					TextColor = Color.White,
				};
				label.SetBinding(Label.TextProperty, new Binding("TestString1"));
				return label;
			});
			var template2 = new DataTemplate(() =>
			{
				var label = new Label
				{
					HorizontalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Center,
					WidthRequest = 50,
					HeightRequest = 50,
					BackgroundColor = Color.White,
					TextColor = Color.Black,
				};
				label.SetBinding(Label.TextProperty, new Binding("TestString1"));
				return label;
			});
			var template3 = new DataTemplate(() =>
			{
				var stack = new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					HorizontalOptions = LayoutOptions.Fill
				};

				var label1 = new Label
				{
					HorizontalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Center,
					WidthRequest = 50,
					HeightRequest = 50,
					BackgroundColor = Color.Black,
					TextColor = Color.White,
				};
				label1.SetBinding(Label.TextProperty, new Binding("TestString1"));

				var label2 = new Label
				{
					HorizontalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Center,
					WidthRequest = 50,
					HeightRequest = 50,
					BackgroundColor = Color.White,
					TextColor = Color.Black,
				};
				label2.SetBinding(Label.TextProperty, new Binding("TestString2"));

				stack.Children.Add(label1);
				stack.Children.Add(label2);

				return stack;
			});

			var src = new List<TestItem>();
			for (int i = 0; i < 100; i++)
				src.Add(new TestItem(i.ToString()));

			cvRed.ItemsLayout = horizontallList;
			cvRed.ItemTemplate = template3;
			cvRed.ItemsSource = src;
			
			cvGreen.ItemsLayout = verticalGrid;
			cvGreen.ItemTemplate = template3;
			cvGreen.ItemsSource = src;
			var spanSetter1 = new SpanSetter(cvGreen);

			cvBlue.ItemsLayout = horizontalGrid;
			cvBlue.ItemTemplate = template3;
			cvBlue.ItemsSource = src;
			var spanSetter2 = new SpanSetter(cvBlue);

			MainPage = new ContentPage
			{
				Content = new StackLayout
				{
					Children =
					{
						cvRed,
						spanSetter1,
						cvGreen,
						spanSetter2,
						cvBlue,
					}
				}
			};
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
