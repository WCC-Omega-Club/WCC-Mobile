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
using Android.Graphics;

namespace WCCMobile
{
    public class BitmapCache
    {
        DiskCache diskCache;
        LRUCache<string, Bitmap> memCache;
        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapCache"/> class.
        /// </summary>
        /// <param name="diskCache">The disk cache.</param>
        BitmapCache(DiskCache diskCache)
        {
            this.diskCache = diskCache;
            this.memCache = new LRUCache<string, Bitmap>(10);
        }
        /// <summary>
        /// Creates the cache.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="cacheName">Name of the cache.</param>
        /// <param name="version">The version.</param>
        /// <returns></returns>
        public static BitmapCache CreateCache(Android.Content.Context context, string cacheName, string version = "1.0")
        {
            return new BitmapCache(DiskCache.CreateCache(context, cacheName, version));
        }
        /// <summary>
        /// Adds or updates the <paramref name="bitmap"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="bmp">The BMP.</param>
        /// <param name="duration">The duration.</param>
        public void AddOrUpdate(string key, Bitmap bitmap, TimeSpan duration)
        {
            diskCache.AddOrUpdate(key, bitmap, duration);
            memCache.Put(key, bitmap);
        }
        /// <summary>
        /// Get's the <paramref name="bitmap"/> associated with the <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="bmp">The BMP.</param>
        /// <returns></returns>
        public bool TryGet(string key, out Bitmap bitmap)
        {
            if ((bitmap = memCache.Get(key)) != null)
                return true;
            return diskCache.TryGet(key, out bitmap);
        }
    }
}