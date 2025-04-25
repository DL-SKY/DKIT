using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

namespace UI.Views.LudusTV
{
    public class LudusTvThumbnailsProvider
    {
        private const int MAX_LOADING_COUNT_SAME_TIME = 6;

        //[Inject] private readonly TimeUpdater _updater;
        //[Inject] private readonly LudusTvLocalSaves _localSave;

        private static readonly string ImagesFolderPath = Path.Combine("ResourcesCache.CacheFolderPath", "LudusTvImages");

        /// <summary>
        /// OnLoaded(string absoluteUri, Sprite image)
        /// </summary>
        //public BAction<string, Sprite> OnLoaded = new BAction<string, Sprite>();

        /// <summary>
        /// Key: absoluteUri, Value: image
        /// </summary>
        private Dictionary<string, Sprite> _thumbnails = new Dictionary<string, Sprite>();
        private List<Texture2D> _cacheTextures = new List<Texture2D>();

        private HashSet<string> _urlsHash = new HashSet<string>();
        private Queue<Uri> _urlsQueue = new Queue<Uri>();
        private int _loadingCount;

        private HashSet<int> _allLinksDefIds = new HashSet<int>();
        private CancellationTokenSource _deleteImagesCts;


        public LudusTvThumbnailsProvider()
        {
            _loadingCount = 0;

            if (!Directory.Exists(ImagesFolderPath))
                Directory.CreateDirectory(ImagesFolderPath);
        }

        private void Update()
        {
            if (_urlsQueue.Count > 0 
                && _loadingCount < MAX_LOADING_COUNT_SAME_TIME)
                LoadNextImageWeb();
        }

        public void SetSpritePathOrUrl(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                //ResetSprite();
            }
            else if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
            {
                //SetSpriteUrl(uri);
            }
            else
            {
                //SetSpritePath(path);
            }
        }

        public void LoadImage(Uri url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            if (!url.IsWellFormedOriginalString() || !url.IsAbsoluteUri)
                throw new UriFormatException($"{nameof(url)} : {url.OriginalString}");


            if (_thumbnails.ContainsKey(url.AbsoluteUri))
            {
                //OnLoaded.Invoke(url.AbsoluteUri, _thumbnails[url.AbsoluteUri]);
                return;
            }


            var cachePath = GetCachePath(url);
            if (File.Exists(cachePath))
            {
                var data = File.ReadAllBytes(cachePath);
                CreateAndInvoke(data, url.AbsoluteUri);
            }
            else if (!_urlsHash.Contains(url.AbsoluteUri))
            {
                _urlsHash.Add(url.AbsoluteUri);
                _urlsQueue.Enqueue(url);
            }
        }

        public void TryDeleteObsoleteImages()
        {
            CancelDeleteImages();
            _deleteImagesCts = new CancellationTokenSource();
            //TryDeleteObsoleteImagesAsync(_deleteImagesCts.Token).Forget();
        }


        private string GetCachePath(Uri url)
        { 
            //return Path.Combine(ImagesFolderPath, $"{DefId.CalculateHash(url.AbsoluteUri)}");
            return Path.Combine(ImagesFolderPath, $"{url.AbsoluteUri}");
        }

        private void CreateAndInvoke(byte[] data, string absoluteUri)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
            texture.LoadImage(data);
            texture.wrapModeU = TextureWrapMode.Clamp;
            texture.wrapModeV = TextureWrapMode.Clamp;
            _cacheTextures.Add(texture);

            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
            sprite.name = absoluteUri;
            
            if (!_thumbnails.ContainsKey(absoluteUri))
                _thumbnails.Add(absoluteUri, sprite);

            _urlsHash.Remove(absoluteUri);

            //OnLoaded.Invoke(absoluteUri, sprite);
        }

        private async void LoadNextImageWeb()
        {
            _loadingCount++;

            var url = _urlsQueue.Dequeue();            
            var cachePath = GetCachePath(url);

            try
            {

                if (!Directory.Exists(ImagesFolderPath))
                    Directory.CreateDirectory(ImagesFolderPath);

#if UNITY_WEBGL
                using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(url))
                {
                    UnityWebRequestAsyncOperation operation = unityWebRequest.SendWebRequest();
                    
                    while (!operation.isDone)
                        await Task.Yield();

                    if (this == null)
                        return;
                    
                    byte[] data = unityWebRequest.downloadHandler.data;
                        
                    File.WriteAllBytes(cachePath, data);                        
                    CreateAndInvoke(data, url.AbsoluteUri);
                }
#else
                var webClient = new WebClient();
                {
                    var data = await webClient.DownloadDataTaskAsync(url);

                    if (this == null)
                        return;

                    File.WriteAllBytes(cachePath, data);
                    CreateAndInvoke(data, url.AbsoluteUri);
                }
#endif

            }
            catch (Exception exception)
            {
                Debug.LogError($"[LudusTvThumbnailsProvider] Failed to load {url}");
                Debug.LogException(exception);

                _urlsHash.Remove(url.AbsoluteUri);
            }
            finally
            {
                _loadingCount--;
            }
        }

        private void CancelDeleteImages()
        {
            if (_deleteImagesCts != null)
                _deleteImagesCts.Cancel();
            _deleteImagesCts = null;
        }

        //private async UniTaskVoid TryDeleteObsoleteImagesAsync(CancellationToken token)
        //{
        //    await UniTask.WaitForSeconds(0.2f, cancellationToken: token);

        //    var datas = _localSave.GetAllLinksData();
        //    if (datas == null || datas.Count < 1)
        //        return;

        //    var thumbnails = YoutubeHelper.ThumbnailsPath;
        //    if (thumbnails == null || thumbnails.Count < 1)
        //        return;

        //    var visual = _context.VisualDefs.LudusTvSettings;
        //    UpdateAllLinksHashset();

        //    foreach (var saveData in datas)
        //    {
        //        foreach (var thumbnail in thumbnails)
        //        {
        //            if (CheckIsObsoleteThumbnail(saveData.Key, thumbnail.Key, _allLinksDefIds, visual.ImageType))
        //            {
        //                if (YoutubeHelper.TryGetThumbnailURI(saveData.Value.YoutubeId, thumbnail.Key, out var url))
        //                {
        //                    var cachePath = GetCachePath(url);
        //                    FileHelper.DeleteIfExists(cachePath);

        //                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
        //                }
        //            }
        //        }
        //    }
        //}

        private void UpdateAllLinksHashset()
        {
            //_allLinksDefIds.Clear();
            //var settings = _context.Defs.LudusTvSettings;
            //foreach (var link in settings.Links)
            //    _allLinksDefIds.Add(new DefId(link.VideoLink));
        }

        //private bool CheckIsObsoleteThumbnail(int linkId, YoutubeThumbnailType type, HashSet<int> allLinks, YoutubeThumbnailType defaultType)
        //{
        //    if (!allLinks.Contains(linkId))
        //        return true;
        //    else if (type != defaultType)
        //        return true;

        //    return false;
        //}


        protected void DisposeInternal()
        {
            //_updater.UnSubscribeUpdate(Update);

            foreach (var thumbnail in _thumbnails)
                UnityEngine.Object.Destroy(thumbnail.Value);
            _thumbnails.Clear();

            foreach (var texture in _cacheTextures)
                UnityEngine.Object.Destroy(texture);
            _cacheTextures.Clear();

            _urlsHash.Clear();
            _urlsQueue.Clear();
            _allLinksDefIds.Clear();

            CancelDeleteImages();
        }

        protected void InitializeInternal()
        {
            //_updater.SubscribeUpdate(Update);
        }
    }
}
