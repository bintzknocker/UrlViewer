# UrlViewer
Sitecore module to display the URL of the selected item in the Quick Info section

The purpose of this module is to display the URL of the selected item in the Quick Info section of the Sitecore UI.

Built and tested against 8.1 update 3

Installation Instructions

DLL
Build the solution in the desired configuration
Copy the UrlViewer.dll into the website/bin directory
After config modifications, copy the UrlViewer.RenderContentEditor.config into the website/app_Config folder

Configuration
The UrlViewer.RenderContentEditor.config file contains all settings for this module.  The config patches in a renderContentEditor processor.
This processor contains 2 configurable areas, paths and sites
Paths -- This section is used to manage which item paths are filtered for the URL to display.  By default the content and media library paths are set
Sites -- This section defines which sites are used when building URLs.  

How it works

When a user selects an item in the Sitecore content tree, the renderContentEditor pipeline fires.  
The UrlViewer processor is called.  The process method does the following

1.Validates that the selected items path is in the configured paths.  If not, the code returns.

2. Sets a HttpContext.Current.Item so that the value can be picked up in later processing

3. Loops over each site defined in the Sites node, sets a new SiteContext and generates a url using the LinkManager.

4. An html table and javascript block is generated.

5. The markup and script block are added to the args.EditorFormatter so that they are written to the Sitecore content editor 
