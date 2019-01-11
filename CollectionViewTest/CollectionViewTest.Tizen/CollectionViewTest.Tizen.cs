using System;
using Tizen;

namespace CollectionViewTest
{
    class Program : global::Xamarin.Forms.Platform.Tizen.FormsApplication
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            
            LoadApplication(new CollectionViewTest());
        }

        static void Main(string[] args)
        {
            var app = new Program();
			global::Xamarin.Forms.Platform.Tizen.Forms.SetFlags("CollectionView_Experimental");
			global::Xamarin.Forms.Platform.Tizen.Forms.Init(app);
            app.Run(args);
        }
    }
}
