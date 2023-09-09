using System;

namespace SampleApp
{

	public partial class MainPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		void Button_OnClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new ExternalFeature.GalleryPage());
		}
	}
}
