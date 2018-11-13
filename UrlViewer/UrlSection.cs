using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.Shell.Applications.ContentEditor.Pipelines.RenderContentEditor;
using Sitecore.Sites;

namespace UrlViewer
{
    public class UrlSection
    {
        #region ..Public Properties
        public virtual string UrlViewer => "UrlViewer";
        #endregion

        #region ..Protected Internal
        protected internal readonly List<string> Paths;
        protected internal readonly List<string> Sites;
        #endregion

        #region ..ctor
        public UrlSection()
        {
            Paths = new List<string>();
            Sites = new List<string>();
        }
        #endregion

        #region ..Public Methods
        public void AddPath(string path)
        {
            Assert.ArgumentNotNullOrEmpty(path, "path");
            Paths.Add(path.ToLower());
        }

        public void AddSite(string site)
        {
            Assert.ArgumentNotNullOrEmpty(site, "site");
            Sites.Add(site.ToLower());
        }

        public void Process(RenderContentEditorArgs args)
        {
            if (args.Item == null)
                return;

            try
            {
                //if the item isnt in a specified path, return
                if (!Paths.Any(x => args.Item.Paths.FullPath.ToLower().StartsWith(x)))
                    return;

                //set a context variable so that if necessary it can be picked up in a custom LinkProvider
                SetHttpContextItem();

                //get the urls for the specified item
                var urls = GetUrlsForItem(args.Item);

                var siteUrls = urls as SiteUrl[] ?? urls.ToArray();
                if (!siteUrls.Any())
                    return;

                //args.EditorFormatter.RenderSectionBegin(args.Parent, "Urls", "Urls", "URLs", "Applications/16x16/information.png", true, false);

                //generate the table that contains the URLs
                var markup = BuildUrlTableMarkup(siteUrls);

                args.EditorFormatter.AddLiteralControl(args.Parent, markup);
                //args.EditorFormatter.RenderSectionEnd(args.Parent, true, false);
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error("Error in UrlViewer.UrlSection: ", ex, this);
            }
        }

        public virtual IEnumerable<SiteUrl> GetUrlsForItem(Item item)
        {
            Assert.IsNotNull(item, "item != null");
            var retVal = new List<SiteUrl>();

            var validSites = Sitecore.Sites.SiteManager.GetSites().Where(s => Sites.Contains(s.Name.ToLower()));

            foreach (var site in validSites)
            {
                using (new SiteContextSwitcher(SiteContextFactory.GetSiteContext(site.Name)))
                {
                    string url = "";
                    url = item.Paths.IsMediaItem ? GetMediaUrl(item) : GetItemUrl(item);
                    retVal.Add(new SiteUrl { SiteName = site.Name, Url = url });
                }
            }

            return retVal;
        }

        public virtual string GetMediaUrl(Item item)
        {
            return MediaManager.GetMediaUrl(item);
        }

        public virtual string GetItemUrl(Item item)
        {
            return LinkManager.GetItemUrl(item);
        }

        public virtual string BuildUrlTableMarkup(SiteUrl[] siteUrls)
        {
            return "<tr class='urlViewer_urls'>" +
              "<td>" +
                   "<table class='urlSectionTable' style='display:none'>" +
                       string.Join(" ", BuildUrlRowMarkup(siteUrls)) +
                   "</table>" +
               "</td>" +
             "</tr>" +
            BuildInjectionScriptTag();
        }

        public virtual string BuildUrlRowMarkup(SiteUrl[] urls)
        {
            var retVal = "";

            for (var i = 0; i < urls.Length; i++)
            {
                if (i == 0)
                    retVal += $"<tr><td>URLs:</td><td>{urls[i].SiteName}: {urls[i].Url}</td></tr>";
                else
                    retVal += $"<tr><td></td><td>{urls[i].SiteName}: {urls[i].Url}</td></tr>";
            }

            return retVal;
        }

        public virtual string BuildInjectionScriptTag()
        {
            return
                "<script type='text/javascript'> var html = jQuery('.urlSectionTable tbody').html(); jQuery('.scEditorQuickInfo tr').last().after(html);" +
                "</script>";
        }

        public virtual void SetHttpContextItem()
        {
            HttpContext.Current.Items.Add(UrlViewer, true);
        }
        #endregion
    }
}