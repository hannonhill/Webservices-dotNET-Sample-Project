using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Serialization;

// import your Cascade WS web reference package (name it 'CascadeWS' when adding to project)
using CascadeTest.CascadeWS;

namespace CascadeTest
{
    public partial class _Default : System.Web.UI.Page
    {
        // handle to the asset operation service
        private AssetOperationHandlerService proxy;
        // identifier element used in read() request
        private identifier id;
        // authentication element used in all requests
        private authentication auth;
        private page myPage;
        private readResult result;
        private path pagePath;
       
        protected void Page_Load(object sender, EventArgs e)
        {   
            System.Web.UI.WebControls.Label label = new Label();
            
            // enclosing in try/catch for easier debugging, primarily
            try
            {
                // get a handle to the asset operation service
                proxy = new AssetOperationHandlerService();

                // contruct a path object referencing the 'about' page from the example.com site
                // note:  change this path to a page that actually exists in your cms instance, if necessary
                pagePath = new path();
                // set the relative path (from the Base Folder) to the asset
                pagePath.path1 = "/about";
                // set the site name of the path object (note: set siteName to 'Global' if the asset is not in a Site)
                pagePath.siteName = "example.com";
               
                // contruct asset identifier used for read() operation
                id = new identifier();
                // set the asset type
                id.type = entityTypeString.page;
                // set asset path (may use either path or id, but never both)
                id.path = pagePath;

                // contruct authentication element to be used in all operations
                auth = new authentication();
                // change username / password as necessary
                auth.username = "admin";
                auth.password = "admin";

                // attempt to read the asset
                result = proxy.read(auth, id);

                // print asset contents to page label
                label.Text = CascadeWSUtils.printAssetContents(result.asset);

                // edit the asset
                // create an empty asset for use with edit() operation
                // (note: this is assuming the authentication user has bypass workflow abilities --
                // if not, you will also need to supply workflowConfig information)
                asset editAsset = new asset();
                editAsset.page = result.asset.page;
                // add some content to the exiting page xhtml
                editAsset.page.xhtml += "<h1>Added via .NET</h1>";
                // must call this method to avoid sending both id and path values in 
                // component references in the asset -- will generate SOAP errors otherwise
                CascadeWSUtils.nullPageValues(editAsset.page);

                // attempt to edit
                operationResult editResult = proxy.edit(auth, editAsset);
                // check results
                label.Text += "<br/><br/>edit success? " + editResult.success + "<br/>message = " + editResult.message;


                // create new asset (using read asset as a model)
                asset newAsset = new asset();
                page newPage = result.asset.page;

                // must call this method to avoid sending both id and path values in 
                // component references in the asset -- will generate SOAP errors otherwise
                CascadeWSUtils.nullPageValues(newPage);

                // since this will be a new asset, change its name
                newPage.name = "new-page-created-via-dot-net";
                // remove id from read asset
                newPage.id = null;
                // remove other system properties brought over from read asset
                newPage.lastModifiedBy = null;
                newPage.lastModifiedDate = null;
                newPage.lastModifiedDateSpecified = false;
                newPage.lastPublishedBy = null;
                newPage.lastPublishedDate = null;
                newPage.lastPublishedDateSpecified = false;
                newPage.pageConfigurations = null;

                newAsset.page = newPage;

                // attempt to create
                createResult createResults = proxy.create(auth, newAsset);
                
                // check create results
                label.Text = label.Text + "<br/><br/>create success? " + createResults.success + "<br/>message = " + createResults.message;

                // debugging -- writes the serialzed XML of the asset element sent in create request to a file
                /*
                // Serializing the returned object
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(newAsset.GetType());

                System.IO.MemoryStream ms = new System.IO.MemoryStream();

                x.Serialize(ms, newAsset);

                ms.Position = 0;

                // Outputting to client

                byte[] byteArray = ms.ToArray();

                Response.Clear();
                Response.AddHeader("Content-Disposition", "attachment; filename=results.xml");

                Response.AddHeader("Content-Length", byteArray.Length.ToString());

                Response.ContentType = "text/xml";

                Response.BinaryWrite(byteArray);
                Response.End();
                 * */
            }
            catch (Exception booboo) { label.Text = "Exception thrown:<br>" + booboo.GetBaseException() + "<br/><br/>STACK TRACE:<br/>" + booboo.StackTrace; }

            WSContent.Controls.Add(label);
        }


    }
}
