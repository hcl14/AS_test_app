using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Webkit;
using Android.Content.Res;
using Android.Provider;

namespace App1
{
    [Activity(Label = "App1", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, GestureDetector.IOnGestureListener
    {
        private WebView web_view1;//banner
        private WebView web_view2;//search / address string
        private WebView web_view3; // web page
        private ImageView horiz_line; // horizontal gradient line, buggy at the moment
        private TextView _textView; //test indicator for gestures
        private static Context mContext; //saving context as static for future

        // first banner contatins animated gif
        private static string bannercontent = "<html><body><img src=\"" + "http://media.giphy.com/media/k29Kh9hu2TZM4/giphy.gif" + "\", width=\"100%\", height=\"100%\"/></body></html>";
        private static string banner2content = "<html><body><div align='center'><img src = 'http://s1.iconbird.com/ico/2013/6/289/w512h5121371656117drive.png'/></div></body></html>";

        // these are two parts that encode google search form for web_view2
        private static string formcontent1 = "<html xmlns='http://www.w3.org/1999/xhtml'><body><form method='get' action='https://www.google.com/search'><input type='text' name='q' style='width:85%;font-size:24px' value='";
        private static string formcontent2 = "'><input type = 'submit' style='width:14%;font-size:24px' value = 'Submit'></form></body></html>";

        // Gesture Detector--------------------------------
        private GestureDetector _gestureDetector;
        public bool OnDown(MotionEvent e)
        {
            return false;
        }
        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY) //press the finger and move rapidly in the desired direction
        {  //we will use this method to switch pages back

            //indication
            _textView = FindViewById<TextView>(Resource.Id.velocity_text_view);  
            _textView.Text = String.Format("Fling velocity: {0} x {1}", velocityX, velocityY);


            if (velocityX > 100 && velocityY < 1000)
            {
                //Calling garbage collector, maybe there are obsolete pages, before creating the new one
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                this.Finish(); // if user scrolls to left horizontally, we finish current page
            }

            return false;
        }
        public void OnLongPress(MotionEvent e) { }
        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return false;
        }
        public void OnShowPress(MotionEvent e) { }
        public bool OnSingleTapUp(MotionEvent e)
        {
            return false;
        }
        
        //https://developer.xamarin.com/recipes/android/other_ux/gestures/create_a_gesture_listener/
        //giving gesture recognition to gesturedetector 
        public override bool OnTouchEvent(MotionEvent e)
        {
            _gestureDetector.OnTouchEvent(e);
            return false;
        }
        // Gesture Detector--------------------------------



        // tiny override, that captures url, created by the web_view2
        public class HelloWebViewClient : WebViewClient
        {

           
            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                //view.LoadUrl(url);

                view.LoadData(formcontent1+url + formcontent2, "text/html", null);

                //Calling garbage collector, maybe there are obsolete pages, before creating the new one
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                // Let's call our Activity recursively again, passing the data to the web_view3
                Intent intent = new Intent(mContext, typeof(MainActivity));
                intent.PutExtra("prev_url", url); //transferring out url to text page
                mContext.StartActivity(intent); //can cuse potential leaks, but its too hard to override basis methods further


                return true;
            }
        }

        //needed to make pages open in App, not in the browser
        //You must implement WebViewClient class and Override shouldOverrideURLLoading() method in this class. 
        // Because webview just opens your "exactly link", if that link redirects other links, android will open default browser for this action.
        //http://stackoverflow.com/questions/5561709/opening-webview-not-in-new-browser
        // web_view3.SetWebViewClient(new WebViewClient());

        private class Callback : WebViewClient
        {  //HERE IS THE MAIN CHANGE. 

            public override bool ShouldOverrideUrlLoading(WebView view, String url)
            {
                return (false);
            }

         }


        // Out Main OnCreate procedure
        protected override void OnCreate(Bundle bundle)
        {
           
            // receiving data from the previous page, if present
            string url = Intent.GetStringExtra("prev_url") ?? "0";


            // gestureDetector instance
            _gestureDetector = new GestureDetector(this);
            _textView = FindViewById<TextView>(Resource.Id.velocity_text_view);
           




            //saving context to dynamical variable to call next activity in the future; Google for "Starting an Android Activity from a static method"
            mContext = this;

            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);



            // Exit Button ---------------------------

            Button button1 = FindViewById<Button>(Resource.Id.button1);

            //doing everything to shut down the app
            button1.Click += delegate {
                MoveTaskToBack(true);
                Process.KillProcess(Process.MyPid());
                System.Environment.Exit(0);
            };
            // Exit Button ---------------------------



            horiz_line = FindViewById<ImageView>(Resource.Id.horizLine);


            // webview - banner
            web_view1 = FindViewById<WebView>(Resource.Id.webView1);
            web_view1.Settings.JavaScriptEnabled = true;
            // web_view1.LoadUrl("http://media.giphy.com/media/k29Kh9hu2TZM4/giphy.gif");

            //fit image inside webview
            String html = bannercontent;
            web_view1.LoadData(html, "text/html", null);

            
            
            
            //main field - google search results

            //main field initialization
            web_view3 = FindViewById<WebView>(Resource.Id.webView3);
            web_view3.Settings.JavaScriptEnabled = true;

 

            web_view3.SetWebViewClient(new Callback());  //http://stackoverflow.com/questions/5561709/opening-webview-not-in-new-browser





            if (url != "0") { web_view3.LoadUrl(url); }
            else { web_view3.LoadData(banner2content, "text/html", null); }


                //address field initialization
                web_view2 = FindViewById<WebView>(Resource.Id.webView2);
            web_view2.Settings.JavaScriptEnabled = true;

            //using just simple form that adds search to google url
            String html2 = formcontent1+formcontent2;
            web_view2.LoadData(html2, "text/html", null);

            //tracking the creation of the url
            // using overriden client
            web_view2.SetWebViewClient(new HelloWebViewClient());
        }

       


    }


}

