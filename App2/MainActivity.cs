﻿using Android.Net;
using Android.App;
using Android.Widget;
using Android.OS;
using Java.IO;
using Android.Graphics;
using System;
using Android.Content;
using Android.Provider;
using Android.Content.PM;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Json;
using RestSharp;
using Java.Nio;
using Newtonsoft.Json;
using System.Threading;

namespace App2
{
    public class Class1
    {
        public List<string> location { get; set; }
        public List<string> emotion { get; set; }
    }

    [Activity(Label = "App2", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        const string url = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize";


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            var textbox = FindViewById<TextView>(Resource.Id.textArea_information);
            if (IsThereAnAppToTakePictures())
            {
                CreateDirectoryForPictures();

                Button button = FindViewById<Button>(Resource.Id.myButton);
                _imageView = FindViewById<ImageView>(Resource.Id.imageView1);
                button.Click += (sender,e) => {
                    TakeAPicture(sender,e);

                };
            }

            var button2 = FindViewById<Button>(Resource.Id.sendAPI);
            button2.Click += async (sender, e) => {

                var apiResponse = await restapi(url, App._file);

                //var x = JsonConvert.DeserializeObject<Class1>(apiResponse);

                textbox.Text = apiResponse;
            };

        }



        //api stuff
        private async Task<string> restapi(string url, Java.IO.File content)
        {
            byte[] byteData = GetImageAsByteArray(content.AbsolutePath);

            const string subscriptionKey = "f6a1c8ee3728467aaf6be30a1f8a793f";
            
            var client = new RestClient(url);

            var request = new RestRequest(Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", subscriptionKey);
            request.AddHeader("Content-Type", "application/octet-stream");

            request.AddParameter ("application/octet-stream", byteData, ParameterType.RequestBody );

            Task<IRestResponse> t = client.ExecuteTaskAsync(request);
            t.Wait();
            var restResponse = await t;

            System.Console.WriteLine(restResponse.Content);
               

            return restResponse.Content;
        }

        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }



        //camera stuff
        private ImageView _imageView;

        public static class App
        {
            public static Java.IO.File _file;
            public static Java.IO.File _dir;
            public static Bitmap bitmap;
        }


        private void CreateDirectoryForPictures()
        {
            App._dir = new Java.IO.File(
                Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryPictures), "CameraAppDemo");
            if (!App._dir.Exists())
            {
                App._dir.Mkdirs();
            }
        }

        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        private void TakeAPicture(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            App._file = new Java.IO.File(App._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(App._file));
            StartActivityForResult(intent, 0);


        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Make it available in the gallery

            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Android.Net.Uri contentUri = Android.Net.Uri.FromFile(App._file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);

            // Display in ImageView. We will resize the bitmap to fit the display.
            // Loading the full sized image will consume to much memory
            // and cause the application to crash.

            int height = Resources.DisplayMetrics.HeightPixels;
            int width = _imageView.Height;
            App.bitmap = App._file.Path.LoadAndResizeBitmap(width, height);
            if (App.bitmap != null)
            {
                _imageView.SetImageBitmap(App.bitmap);
                App.bitmap = null;
            }


            // Dispose of the Java side bitmap.
            GC.Collect();
        }

        
    }

    public static class BitmapHelpers
    {
        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
        {
            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileName, options);

            // Next we calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }

            // Now we will load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

            return resizedBitmap;
        }
    }
}

