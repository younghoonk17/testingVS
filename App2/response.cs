using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace App2
{
    public class response
    {
        public IList<string> tags { get; set; }
        public string requestId { get; set; }
        public IList<string> metadata { get; set; }
    }


}