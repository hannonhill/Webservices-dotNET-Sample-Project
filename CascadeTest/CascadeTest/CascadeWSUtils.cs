/**
 * Created on Feb 8, 2011 by Brent Arrington
 *
 * THE PROGRAM IS DISTRIBUTED IN THE HOPE THAT IT WILL BE USEFUL, BUT WITHOUT ANY WARRANTY. IT IS PROVIDED "AS IS" 
 * WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THE ENTIRE RISK AS TO THE QUALITY AND PERFORMANCE OF THE 
 * PROGRAM IS WITH YOU. SHOULD THE PROGRAM PROVE DEFECTIVE, YOU ASSUME THE COST OF ALL NECESSARY SERVICING, REPAIR OR 
 * CORRECTION.
 * 
 * IN NO EVENT UNLESS REQUIRED BY APPLICABLE LAW THE AUTHOR WILL BE LIABLE TO YOU FOR DAMAGES, INCLUDING ANY GENERAL, SPECIAL, 
 * INCIDENTAL OR CONSEQUENTIAL DAMAGES ARISING OUT OF THE USE OR INABILITY TO USE THE PROGRAM (INCLUDING BUT NOT LIMITED TO LOSS 
 * OF DATA OR DATA BEING RENDERED INACCURATE OR LOSSES SUSTAINED BY YOU OR THIRD PARTIES OR A FAILURE OF THE PROGRAM TO OPERATE 
 * WITH ANY OTHER PROGRAMS), EVEN IF THE AUTHOR HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.
 * 
 * Please feel free to distribute this code in any way, with or without this notice.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
// import your Cascade web reference package (name it 'CascadeWS' when adding to project)
using CascadeTest.CascadeWS;

namespace CascadeTest
{
    /*
     * Because of a limitation with Apache Axis, the data received when doing
     * a read on an asset is not able to be directly sent back to the server
     * as-is because the server actually sends more data than necessary. These
     * functions will go through assets and null out the unnecessary items,
     * making the asset able to be sent back to the server.
     *
     * In essence, the server sends both the id and path information for a relationship,
     * however the server will only accept either the id or the path, but not both. These
     * methods null out the applicable relationship paths in favor of the ids.
     *
     * When editing a page, the order of the steps should go like this:
     * 1. Read page (send the read request)
     * 2. Get all the needed data from the page (for example: "string myTitle = myPage.metadata.title;")
     * 3. Null page values (call the appropriate method here)
     * 4. Set all the needed data to the page (whatever needs to be modified)
     * 5. Edit the page (send the edit request)
     */
    public class CascadeWSUtils
    {
        /**
        * Nulls out base asset's values
        * 
        * @param baseAsset
        */
        private static void nullAssetValues(baseasset baseAsset)
        {
            //Never, ever send an entity type. This will lead to an error.
            baseAsset.entityType = null;
        }

        /**
            * Nulls out folder contained asset's values
            * 
            * @param folderContained
            */
        private static void nullFolderContainedValues(foldercontainedasset folderContained)
        {
            nullAssetValues(folderContained);
            //Null out the various relationship paths in favor of the ids 
            if (folderContained.parentFolderId != null)
                folderContained.parentFolderPath = null;
            if (folderContained.id != null)
                folderContained.path = null;
            if (folderContained.siteId != null)
                folderContained.siteName = null;
        }

        /**
            * Nulls out publishable asset's values
            * 
            * @param publishable
            */
        private static void nullPublishableValues(publishableasset publishable)
        {
            nullExpiringValues(publishable);
            if (publishable.expirationFolderId != null)
                publishable.expirationFolderPath = null;
        }

        /**
            * Nulls out dublin aware values
            * 
            * @param dublinAware
            */
        private static void nullDublinAwareValues(dublinawareasset dublinAware)
        {
            nullFolderContainedValues(dublinAware);
            if (dublinAware.metadataSetId != null)
                dublinAware.metadataSetPath = null;
        }

        /**
            * Nulls out expiring values
            * 
            * @param expiring
            */
        private static void nullExpiringValues(expiringasset expiring)
        {
            nullDublinAwareValues(expiring);
            if (expiring.expirationFolderId != null)
                expiring.expirationFolderPath = null;
        }

        /**
            * Because of a limitation with Apache Axis, the data received when doing
            * a read on an asset is not able to be directly sent back to the server
            * as-is because the server actually sends more data than necessary. This
            * function will go through a page asset and null out the unnecessary items,
            * making the asset able to be sent back to the server.
            * 
            * In essence, the server sends both the id and path information for a relationship,
            * however the server will only accept either the id or the path, but not both. This
            * method nulls out the applicable relationship paths in favor of the ids. 
            * 
            * @param folder the folder whose data will be intelligently nulled out to ensure
            *      it can be sent back to the server.
            */
        public static void nullFolderValues(folder folder)
        {
            nullPublishableValues(folder);
            folder.children = null;
        }

        /**
            * Because of a limitation with Apache Axis, the data received when doing
            * a read on an asset is not able to be directly sent back to the server
            * as-is because the server actually sends more data than necessary. This
            * function will go through a page asset and null out the unnecessary items,
            * making the asset able to be sent back to the server.
            * 
            * In essence, the server sends both the id and path information for a relationship,
            * however the server will only accept either the id or the path, but not both. This
            * method nulls out the applicable relationship paths in favor of the ids. 
            * 
            * @param page the page whose data will be intelligently nulled out to ensure
            *      it can be sent back to the server.
            */
        public static void nullPageValues(page page)
        {
            nullPublishableValues(page);

            // If the page has a content type, null out the configuration set
            if ((page.contentTypeId != null) || (page.contentTypePath != null))
            {
                page.configurationSetId = null;
                page.configurationSetPath = null;
                if (page.contentTypeId != null)
                    page.contentTypePath = null;
            }
            else
            {
                if (page.configurationSetId != null)
                    page.configurationSetPath = null;
            }

            //If the page has structured data, null out the structured data
            //relationships as well
            structureddata sData = page.structuredData;
            if (sData != null)
            {
                if (sData.definitionId != null)
                    sData.definitionPath = null;
                structureddatanode[] nodes = sData.structuredDataNodes;

                if (nodes != null)
                {
                    nullStructuredData(nodes);
                }
            }

            nullPageConfigurationValues(page.pageConfigurations);
        }

        /*
        private static void nullPageConfigurationValues(pageConfigurations configs)
        {
            //Null out all the page configuration relationship information
            if (configs != null && configs.pageConfiguration != null)
            {
                for (int i = 0; i < configs.pageConfiguration.Length; i++)
                {
                    if (configs.pageConfiguration[i].formatId != null)
                        configs.pageConfiguration[i].formatPath = null;
                    if (configs.pageConfiguration[i].templateId != null)
                        configs.pageConfiguration[i].templatePath = null;
                    configs.pageConfiguration[i].entityType = null;

                    // fix page regions
                    nullPageRegionValues(configs.pageConfiguration[i].pageRegions);
                }
            }
        }*/
        
        private static void nullPageConfigurationValues(ArrayList configsList)
        {   
            object[] configs = configsList.ToArray();
            //Null out all the page configuration relationship information
            if (configs != null)
            {
                foreach (object config in configs)
                {
                    try
                    {
                        pageConfiguration pgConf = (pageConfiguration)config;
                        if (pgConf.formatId != null)
                            pgConf.formatPath = null;
                        if (pgConf.templateId != null)
                            pgConf.templatePath = null;
                        pgConf.entityType = null;

                        // fix page regions
                        nullPageRegionValues(pgConf.pageRegions);

                    }
                    catch (Exception e)
                    {
                        
                    }

                    try
                    {
                        pageConfiguration2 pgConf = (pageConfiguration2)config;
                        if (pgConf.formatId != null)
                            pgConf.formatPath = null;
                        if (pgConf.templateId != null)
                            pgConf.templatePath = null;
                        pgConf.entityType = null;

                        // fix page regions
                        nullPageRegionValues(pgConf.pageRegions);
                    }
                    catch (Exception e)
                    {

                    }
             
                }
            }
        }

        /**
            * Nulls out page region values
            * 
            * @param pRegs
            */
        private static void nullPageRegionValues(pageRegion[] pRegs)
        {
            if (pRegs != null)
            {
                for (int j = 0; j < pRegs.Length; j++)
                {
                    if (pRegs[j].blockId != null)
                        pRegs[j].blockPath = null;
                    if (pRegs[j].formatId != null)
                        pRegs[j].formatPath = null;
                    pRegs[j].entityType = null;
                }
            }
        }

        /**
            * Nulls out unneeded values from a ScriptFormat.
            * 
            * @param scriptFormat
            */
        public static void nullScriptFormatValues(scriptFormat scriptFormat)
        {
            nullFolderContainedValues(scriptFormat);
        }

        /**
            * Nulles out unneeded values from a XsltFormat.
            * @param xsltFormat
            */
        public static void nullXsltFormatValues(xsltFormat xsltFormat)
        {
            nullFolderContainedValues(xsltFormat);
        }

        /**
            * Nulls out unneeded values in an array of StructuredDataNode
            * objects
            *
            * @param sDataNodes
            */
        private static void nullStructuredData(structureddatanode[] sDataNodes)
        {
            if (sDataNodes != null)
            {
                for (int k = 0; k < sDataNodes.Length; k++)
                {
                    if (structureddatatype.asset == sDataNodes[k].type)
                    {
                        sDataNodes[k].text = null;

                        if (sDataNodes[k].assetType == structureddataassettype.block)
                        {
                            if (sDataNodes[k].blockId == null && sDataNodes[k].blockPath == null)
                            {
                                sDataNodes[k].blockPath = "";
                            }
                            else
                            {
                                sDataNodes[k].blockId = null;
                            }
                        }
                        else if (sDataNodes[k].assetType == structureddataassettype.file)
                        {
                            if (sDataNodes[k].fileId == null && sDataNodes[k].filePath == null)
                            {
                                sDataNodes[k].filePath = "";
                            }
                            else
                            {
                                sDataNodes[k].fileId = null;
                            }
                        }
                        else if (sDataNodes[k].assetType == structureddataassettype.page)
                        {
                            if (sDataNodes[k].pageId == null && sDataNodes[k].pagePath == null)
                            {
                                sDataNodes[k].pagePath = "";
                            }
                            else
                            {
                                sDataNodes[k].pageId = null;
                            }
                        }
                        else if (sDataNodes[k].assetType == structureddataassettype.symlink)
                        {
                            if (sDataNodes[k].symlinkId == null && sDataNodes[k].symlinkPath == null)
                            {
                                sDataNodes[k].symlinkPath = "";
                            }
                            else
                            {
                                sDataNodes[k].symlinkId = null;
                            }
                        }
                    }
                    else if (structureddatatype.group == sDataNodes[k].type)
                    {
                        structureddatanode[] sDataNodeArray = sDataNodes[k].structuredDataNodes;
                        nullStructuredData(sDataNodeArray);
                        sDataNodes[k].text = null;
                        sDataNodes[k].assetType = null;
                    }
                    else if (structureddatatype.text == sDataNodes[k].type)
                    {
                        sDataNodes[k].assetType = null;
                        sDataNodes[k].structuredDataNodes = null;
                        if (sDataNodes[k].text == null)
                        {
                            sDataNodes[k].text = "";
                        }
                    }
                    else
                    {
                        sDataNodes[k].assetType = null;
                        sDataNodes[k].text = null;
                    }
                }
            }
        }
        /*
        private static void nullStructuredData(structureddatanodes sDataNodes)
        {
            if (sDataNodes != null & sDataNodes.structuredDataNode != null)
            {
                for (int k = 0; k < sDataNodes.structuredDataNode.Length; k++)
                {
                    if (structureddatatype.asset == sDataNodes.structuredDataNode[k].type)
                    {
                        sDataNodes.structuredDataNode[k].text = null;

                        if (sDataNodes.structuredDataNode[k].assetType == structureddataassettype.block)
                        {
                            if (sDataNodes.structuredDataNode[k].blockId == null && sDataNodes.structuredDataNode[k].blockPath == null)
                            {
                                sDataNodes.structuredDataNode[k].blockPath = "";
                            }
                            else
                            {
                                sDataNodes.structuredDataNode[k].blockId = null;
                            }
                        }
                        else if (sDataNodes.structuredDataNode[k].assetType == structureddataassettype.file)
                        {
                            if (sDataNodes.structuredDataNode[k].fileId == null && sDataNodes.structuredDataNode[k].filePath == null)
                            {
                                sDataNodes.structuredDataNode[k].filePath = "";
                            }
                            else
                            {
                                sDataNodes.structuredDataNode[k].fileId = null;
                            }
                        }
                        else if (sDataNodes.structuredDataNode[k].assetType == structureddataassettype.page)
                        {
                            if (sDataNodes.structuredDataNode[k].pageId == null && sDataNodes.structuredDataNode[k].pagePath == null)
                            {
                                sDataNodes.structuredDataNode[k].pagePath = "";
                            }
                            else
                            {
                                sDataNodes.structuredDataNode[k].pageId = null;
                            }
                        }
                        else if (sDataNodes.structuredDataNode[k].assetType == structureddataassettype.symlink)
                        {
                            if (sDataNodes.structuredDataNode[k].symlinkId == null && sDataNodes.structuredDataNode[k].symlinkPath == null)
                            {
                                sDataNodes.structuredDataNode[k].symlinkPath = "";
                            }
                            else
                            {
                                sDataNodes.structuredDataNode[k].symlinkId = null;
                            }
                        }
                    }
                    else if (structureddatatype.group == sDataNodes.structuredDataNode[k].type)
                    {
                        structureddatanodes sDataNodeArray = sDataNodes.structuredDataNode[k].structuredDataNodes;
                        nullStructuredData(sDataNodeArray);
                        sDataNodes.structuredDataNode[k].text = null;
                        sDataNodes.structuredDataNode[k].assetType = null;
                    }
                    else if (structureddatatype.text == sDataNodes.structuredDataNode[k].type)
                    {
                        sDataNodes.structuredDataNode[k].assetType = null;
                        sDataNodes.structuredDataNode[k].structuredDataNodes = null;
                        if (sDataNodes.structuredDataNode[k].text == null)
                        {
                            sDataNodes.structuredDataNode[k].text = "";
                        }
                    }
                    else
                    {
                        sDataNodes.structuredDataNode[k].assetType = null;
                        sDataNodes.structuredDataNode[k].text = null;
                    }
                }
            }
        }*/

        /**
            * Nulls out unneeded properties on File objects.
            * 
            * @param file
            */
        public static void nullFileValues(file file)
        {
            nullPublishableValues(file);
            if ((file.text != null) && (file.text != ""))
                file.data = null;
            else
                file.text = null;
        }

        /**
            * Nulls out unneeded properties on PageConfigurationSetContainer objects. 
            * @param pcsc
            */
        public static void nullPageConfigurationSetContainerValues(pageConfigurationSetContainer pcsc)
        {
            nullContaineredValues(pcsc);
        }

        /**
            * Nulls out unneeded properties on ContaineredAsset objects.
            * @param asset
            */
        public static void nullContaineredValues(containeredasset asset)
        {
            nullAssetValues(asset);

            if (asset.parentContainerId != null)
                asset.parentContainerPath = null;
            if (asset.siteId != null)
                asset.siteName = null;
        }

        /**
            * Nulls out unneeded properties of a TransportContainer object
            *  
            * @param asset
            */
        public static void nullTransportContainerValues(transportContainer asset)
        {
            nullContaineredValues(asset);

            asset.children = null;
        }

        /**
            * Because of a limitation with Apache Axis, the data received when doing
            * a read on an asset is not able to be directly sent back to the server
            * as-is because the server actually sends more data than necessary. This
            * function will go through a page asset and null out the unnecessary items,
            * making the asset able to be sent back to the server.
            * 
            * In essence, the server sends both the id and path information for a relationship,
            * however the server will only accept either the id or the path, but not both. This
            * method nulls out the applicable relationship paths in favor of the ids. 
            * 
            * @param user the user whose data will be intelligently nulled out to ensure
            *      it can be sent back to the server.
            */
        public static void nullUserValues(user user)
        {
            user.entityType = null;
        }

        /**
            * Nulls out unneeded properties of a FeedBlock object
            * 
            * @param feedBlock
            */
        public static void nullFeedBlockValues(feedBlock feedBlock)
        {
            nullExpiringValues(feedBlock);
        }

        /**
            * Nulls out unneeded properties of an IndexBlock object
            * 
            * @param indexBlock
            */
        public static void nullIndexBlockValues(indexBlock indexBlock)
        {
            nullExpiringValues(indexBlock);

            indexblocktype indexBlockType = indexBlock.indexBlockType;
            if (indexBlockType == null)
                indexBlockType = indexblocktype.folder;

            if (indexBlockType == indexblocktype.folder)
            {
                if (indexBlock.indexedFolderId == null && indexBlock.indexedFolderPath == null)
                    indexBlock.indexedFolderPath = "";
                else if (indexBlock.indexedFolderId != null)
                    indexBlock.indexedFolderPath = null;
                indexBlock.indexedContentTypeId = null;
                indexBlock.indexedContentTypePath = null;
            }
            else if (indexBlockType == indexblocktype.contenttype)
            {
                if (indexBlock.indexedContentTypeId == null && indexBlock.indexedContentTypePath == null)
                    indexBlock.indexedContentTypePath = "";
                else if (indexBlock.indexedContentTypeId != null)
                    indexBlock.indexedContentTypePath = null;
                indexBlock.indexedFolderId = null;
                indexBlock.indexedFolderPath = null;
            }
        }

        /**
            * Nulls out unneeded properties of a Template object
            * 
            * @param template
            */
        public static void nullTemplateValues(template template)
        {
            nullFolderContainedValues(template);

            if (template.targetId != null)
                template.targetPath = null;

            nullPageRegionValues(template.pageRegions);
        }

        /**
            * Nulls out unneeded properties of a Reference object
            * 
            * @param reference
            */
        public static void nullReferenceValues(reference reference)
        {
            nullFolderContainedValues(reference);

            if (reference.referencedAssetId != null)
                reference.referencedAssetPath = null;
        }

        /**
            * Nulls out unneeded properties of a TextBlock object
            * 
            * @param textBlock
            */
        public static void nullTextBlockValues(textBlock textBlock)
        {
            nullExpiringValues(textBlock);
        }

        /**
            * Nulls out unneeded properties of an XhtmlStructuredDataBlock object
            * 
            * @param structuredDataBlock
            */
        public static void nullXhtmlDataDefinitionBlockValues(xhtmlDataDefinitionBlock structuredDataBlock)
        {
            //If the block has structured data, null out the structured data
            //relationships as well
            structureddata sData = structuredDataBlock.structuredData;
            if (sData != null)
            {
                if (sData.definitionId != null)
                    sData.definitionPath = null;
                structureddatanode[] nodes = sData.structuredDataNodes;

                if (nodes != null)
                {
                    nullStructuredData(nodes);
                }
            }

            nullExpiringValues(structuredDataBlock);
        }

        /**
            * Nulls out unneeded properties of an XmlBlock object
            * 
            * @param xmlBlock
            */
        public static void nullXmlBlockValues(xmlBlock xmlBlock)
        {
            nullExpiringValues(xmlBlock);
        }

        /**
            * Nulls out unneeded properties of a Symlink object
            * 
            * @param symlink
            */
        public static void nullSymlinkValues(symlink symlink)
        {
            nullExpiringValues(symlink);
        }

        /**
            * Nulls out unneeded properties of a PageConfigurationSet object.
            * @param pcs
            */
        public static void nullPageConfigurationSetValues(pageConfigurationSet pcs)
        {
            nullPageConfigurationValues(pcs.pageConfigurations);
            nullContaineredValues(pcs);
        }

        /**
            * Nulls out unneeded properties of a Site object.
            * @param site
            */
        public static void nullSiteValues(site site)
        {
            if (site.cssFileId != null && site.cssFileId != "")
                site.cssFilePath = null;

            if (site.defaultMetadataSetId != null && site.defaultMetadataSetId != "")
                site.defaultMetadataSetPath = null;

            if (site.siteAssetFactoryContainerId != null && site.siteAssetFactoryContainerId != "")
                site.siteAssetFactoryContainerPath = null;

            if (site.siteStartingPageId != null && site.siteStartingPageId != "")
                site.siteStartingPagePath = null;

            if (site.roleAssignments != null)
            {
                foreach (roleassignment assignment in site.roleAssignments)
                {
                    if (assignment.roleId != null && assignment.roleId != "")
                        assignment.roleName = null;
                }
            }

            site.entityType = null;
        }

        /**
            * Nulls out unneeded properties of a Role object
            * 
            * @param role
            */
        public static void nullRoleValues(role role)
        {
            nullAssetValues(role);
        }

        /**
            * Nulls out all the un-required 
            * @param ms
            */
        public static void nullMetadataSetValues(metadataSet ms)
        {
            nullAssetValues(ms);
            nullContaineredValues(ms);
        }

        /**
            * Nulls out unneeded values for Destination objects before
            * submitting for edit or create.
            * 
            * @param dest
            */
        public static void nullDestinationValues(destination dest)
        {
            nullAssetValues(dest);
            // couldn't call nullContaineredValues because Destination is
            // not in the ContaineredAsset hierarchy probably because it is
            // used both for Sites - where it does have containers - and in Global -
            // where it does not
            if (dest.parentContainerId != null)
                dest.parentContainerPath = null;
            if (dest.siteId != null)
                dest.siteName = null;
            if (dest.transportId != null)
                dest.transportPath = null;
        }

        /**
            * Nulls out unneeded values for AssetFactory objects before
            * submitting for edit or create.
            * 
            * @param af
            */
        public static void nullAssetFactoryValues(assetFactory af)
        {
            nullAssetValues(af);
            nullContaineredValues(af);
            if (af.baseAssetId != null && af.baseAssetId != "")
                af.baseAssetPath = null;
            if (af.placementFolderId != null && af.placementFolderId != "")
                af.placementFolderPath = null;
            if (af.workflowDefinitionId != null && af.workflowDefinitionId != "")
                af.workflowDefinitionPath = null;
        }

        /**
            * Nulls out unneeded values for AssetFactoryContainer objects before
            * submitting for edit or create.
            * 
            * @param afc
            */
        public static void nullAssetFactoryContainerValues(assetFactoryContainer afc)
        {
            nullAssetValues(afc);
            nullContaineredValues(afc);
            afc.children = null;
        }

        /**
            * Nulls out all un-required WorkflowDefinition fields
            * 
            * @param wf
            */
        public static void nullWorkflowDefinitionValues(workflowDefinition wf)
        {
            nullAssetValues(wf);
            nullContaineredValues(wf);
        }

        /**
            * Fetches id of the input Site object. Returns <code>null</code>
            * if the input object is <code>null</code>
            * 
            * @param site
            * @return Returns the id of the input Site object or null 
            */
        public static String getSiteId(site site)
        {
            if (site == null)
                return null;

            return site.id;
        }

        private static void nullConnectorValues(connector c)
        {
            nullContaineredValues(c);
            connectorcontenttypelink[] links = c.connectorContentTypeLinks;
            foreach (connectorcontenttypelink link in links)
            {
                if (link.contentTypeId != null && link.contentTypeId != "")
                    link.contentTypePath = null;
                if (link.pageConfigurationId != null && link.pageConfigurationId != "")
                    link.pageConfigurationName = null;
            }
        }

        /**
            * Nulls out unneeded values from a TwitterConnector.
            * 
            * @param tc
            */
        public static void nullTwitterConnectorValues(twitterConnector tc)
        {
            nullConnectorValues(tc);
            if (tc.destinationId != null && tc.destinationId != "")
                tc.destinationPath = null;
        }

        /**
            * Nulls out unneeded values from a WordPressConnector.
            * 
            * @param wpc
            */
        public static void nullWordPressConnectorValues(wordPressConnector wpc)
        {
            nullConnectorValues(wpc);
        }

        /**
            * Nulls out unneeded values from a Group
            * 
            * @param g
            */
        public static void nullGroupValues(group g)
        {
            g.entityType = null;
            if (g.groupAssetFactoryContainerId != null && g.groupAssetFactoryContainerId != "")
                g.groupAssetFactoryContainerPath = null;
            if (g.groupBaseFolderId != null && g.groupBaseFolderId != "")
                g.groupBaseFolderPath = null;
            if (g.groupStartingPageId != null && g.groupStartingPageId != "")
                g.groupStartingPagePath = null;
        }

        /**
            * Nulls out unneeded values from a PublishSet
            * 
            * @param ps
            */
        public static void nullPublishSetValues(publishSet ps)
        {
            nullContaineredValues(ps);
            foreach (identifier i in ps.files)
                nullIdentifierValues(i);
            foreach (identifier i in ps.pages)
                nullIdentifierValues(i);
            foreach (identifier i in ps.folders)
                nullIdentifierValues(i);
        }

        /**
            * Nulls out unneeded values from an Identifier
            * 
            * @param i
            */
        private static void nullIdentifierValues(identifier i)
        {
            if (i.id != null && i.id != "")
                i.path = null;
        }

        /**
            * Nulls out unneeded values from a Target
            * 
            * @param t
            */
        public static void nullTargetValues(target t)
        {
            nullAssetValues(t);
            if (t.baseFolderId != null && t.baseFolderId != "")
                t.baseFolderPath = null;
            if (t.cssFileId != null && t.cssFileId != "")
                t.cssFilePath = null;
            if (t.parentTargetId != null && t.parentTargetId != "")
                t.parentTargetPath = null;
        }

        // ----------------------------------------------------------------------------------------------------------------
        // debugging utilities:

        /*
         * Returns an XHTML string containing a listing of the provided asset's structure.
         * For use in debugging.
         * (Only implemented for page assets at the moment)
         */
        public static string printAssetContents(asset a)
        {
            string contents = "<ul>";

            contents += "<li>WorkflowConfiguration:" + printWorkflowConfiguration(a.workflowConfiguration) + "</li>";

            if (a.page != null)
            {
                contents += "<li>Page:</li><ul>";
                page p = a.page;
                contents += "<li>name = " + p.name + "</li>";
                contents += "<li>id = " + p.id + "</li>";
                contents += "<li>configurationSetId = " + p.configurationSetId + "</li>";
                contents += "<li>configurationSetPath = " + p.configurationSetPath + "</li>";
                contents += "<li>contentTypeId = " + p.contentTypeId + "</li>";
                contents += "<li>contentTypePath = " + p.contentTypePath + "</li>";
                contents += "<li>entityType = " + p.entityType + "</li>";
                contents += "<li>expirationFolderId = " + p.expirationFolderId + "</li>";
                contents += "<li>expirationFolderPath = " + p.expirationFolderPath + "</li>";
                contents += "<li>expirationFolderRecycled = " + p.expirationFolderRecycled + "</li>";
                contents += "<li>expirationFolderRecycledSpecified = " + p.expirationFolderRecycledSpecified + "</li>";
                contents += "<li>lastModifiedBy = " + p.lastModifiedBy + "</li>";
                contents += "<li>lastModifiedDate = " + p.lastModifiedDate + "</li>";
                contents += "<li>lastModifiedDateSpecified = " + p.lastModifiedDateSpecified + "</li>";
                contents += "<li>lastPublishedBy = " + p.lastPublishedBy + "</li>";
                contents += "<li>lastPublishedDate = " + p.lastPublishedDate + "</li>";
                contents += "<li>lastPublishedDateSpecified = " + p.lastPublishedDateSpecified + "</li>";
                contents += "<li>metadata = " + p.metadata + "</li>";
                contents += "<li>metadataSetId = " + p.metadataSetId + "</li>";
                contents += "<li>metadataSetPath = " + p.metadataSetPath + "</li>";
                contents += "<li>pageConfigurations = " + printPageConfigurations(p.pageConfigurations) + "</li>";
                contents += "<li>parentFolderId = " + p.parentFolderId + "</li>";
                contents += "<li>parentFolderpath = " + p.parentFolderPath + "</li>";
                contents += "<li>path = " + p.path + "</li>";
                contents += "<li>shouldBeIndexed = " + p.shouldBeIndexed + "</li>";
                contents += "<li>shouldBeIndexedSpecified = " + p.shouldBeIndexedSpecified + "</li>";
                contents += "<li>shouldBePublished = " + p.shouldBePublished + "</li>";
                contents += "<li>shouldBePublishedSpecified = " + p.shouldBePublishedSpecified + "</li>";
                contents += "<li>siteId = " + p.siteId + "</li>";
                contents += "<li>siteName = " + p.siteName + "</li>";
                contents += "<li>structuredData = " + p.structuredData + "</li>";
                contents += "<li>xhtml = " + p.xhtml + "</li>";

                contents += "</ul>";
            }

            contents += "</ul>";

            return contents;
        }

        public static string printWorkflowConfiguration(workflowconfiguration config)
        {
            string val = "<ul>";

            if (config != null)
            {
                val += "<li>workflowComments = " + config.workflowComments + "</li>";
                val += "<li>workflowDefinitionId = " + config.workflowDefinitionId + "</li>";
                val += "<li>workflowDefinitionPath = " + config.workflowDefinitionPath + "</li>";
                val += "<li>workflowName = " + config.workflowName + "</li>";
                val += "<li>workflowStepConfigurations = " + config.workflowStepConfigurations + "</li>";
            }
            else
            {
                val += "<li>null</li>";
            }


            val += "</ul>";
            return val;
        }
       
        public static string printPageConfigurations(ArrayList configsList)
        {
            string val = "Page Configurations:";

            object[] configs = configsList.ToArray();

            if (configs != null)
            {
                foreach (object configObj in configs)
                {
                    try
                    {
                        pageConfiguration config = (pageConfiguration)configObj;
                        val += "<ul><li>CONFIG:</li><li>";
                        val += "<ul>";

                        val += "<li>defaultConfiguration = " + config.defaultConfiguration + "</li>";
                        val += "<li>entityType = " + config.entityType + "</li>";
                        val += "<li>formatId = " + config.formatId + "</li>";
                        val += "<li>formatPath = " + config.formatPath + "</li>";
                        val += "<li>formatRecycled = " + config.formatRecycled + "</li>";
                        val += "<li>formatRecycledSpecified = " + config.formatRecycledSpecified + "</li>";
                        val += "<li>id = " + config.id + "</li>";
                        val += "<li>includeXMLDeclaration = " + config.includeXMLDeclaration + "</li>";
                        val += "<li>includeXMLDeclarationSpecified = " + config.includeXMLDeclarationSpecified + "</li>";
                        val += "<li>name = " + config.name + "</li>";
                        val += "<li>outputExtension = " + config.outputExtension + "</li>";
                        val += "<li>pageRegions = " + config.pageRegions + "</li>";
                        val += "<li>publishable = " + config.publishable + "</li>";
                        val += "<li>publishableSpecified = " + config.publishableSpecified + "</li>";
                        val += "<li>serializationType = " + config.serializationType + "</li>";
                        val += "<li>serializationTypeSpecified = " + config.serializationTypeSpecified + "</li>";
                        val += "<li>templateId = " + config.templateId + "</li>";
                        val += "<li>templatePath = " + config.templatePath + "</li>";

                        val += "</ul></li></ul>";
                    }
                    catch (Exception e)
                    {

                    }

                    try
                    {
                        pageConfiguration2 config = (pageConfiguration2)configObj;
                        val += "<ul><li>CONFIG:</li><li>";
                        val += "<ul>";

                        val += "<li>defaultConfiguration = " + config.defaultConfiguration + "</li>";
                        val += "<li>entityType = " + config.entityType + "</li>";
                        val += "<li>formatId = " + config.formatId + "</li>";
                        val += "<li>formatPath = " + config.formatPath + "</li>";
                        val += "<li>formatRecycled = " + config.formatRecycled + "</li>";
                        val += "<li>formatRecycledSpecified = " + config.formatRecycledSpecified + "</li>";
                        val += "<li>id = " + config.id + "</li>";
                        val += "<li>includeXMLDeclaration = " + config.includeXMLDeclaration + "</li>";
                        val += "<li>includeXMLDeclarationSpecified = " + config.includeXMLDeclarationSpecified + "</li>";
                        val += "<li>name = " + config.name + "</li>";
                        val += "<li>outputExtension = " + config.outputExtension + "</li>";
                        val += "<li>pageRegions = " + config.pageRegions + "</li>";
                        val += "<li>publishable = " + config.publishable + "</li>";
                        val += "<li>publishableSpecified = " + config.publishableSpecified + "</li>";
                        val += "<li>serializationType = " + config.serializationType + "</li>";
                        val += "<li>serializationTypeSpecified = " + config.serializationTypeSpecified + "</li>";
                        val += "<li>templateId = " + config.templateId + "</li>";
                        val += "<li>templatePath = " + config.templatePath + "</li>";

                        val += "</ul></li></ul>";
                    }
                    catch (Exception e)
                    {

                    }

                }
            }
            else
            {
                val += " IS NULL";
            }

            return val;
        }
    }

    
}