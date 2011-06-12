﻿/***************************************************************************
 *  Copyright (C) 2010 by Peter L Jones                                    *
 *  pljones@users.sf.net                                                   *
 *                                                                         *
 *  This file is part of the Sims 3 Package Interface (s3pi)               *
 *                                                                         *
 *  s3pi is free software: you can redistribute it and/or modify           *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  s3pi is distributed in the hope that it will be useful,                *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with s3pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using s3pi.Interfaces;

namespace ObjectCloner.SplitterComponents
{
    public partial class Search : UserControl
    {

        ListViewColumnSorter lvwColumnSorter;

        MainForm.updateProgressCallback updateProgressCB;
        MainForm.listViewAddCallBack listViewAddCB;



        public Search()
        {
            InitializeComponent();
            lvwColumnSorter = new ListViewColumnSorter();
            this.listView1.ListViewItemSorter = lvwColumnSorter;
            Search_LoadListViewSettings();
            cbCatalogType.Items.AddRange(new string[] {
                "Any",
                "Fence", "Stairs", "Proxy Product", "Terrain Geometry Brush",
                "Railing", "Terrain Paint Brush", "Fireplace", "Terrain Water Brush",
                "Fountain / Pool",
                "Foundation", "Normal Object", "Wall/Floor Pattern", "Wall Style",
                "Roof Style", "Modular Resource", "Roof Pattern",
            });
            cbCatalogType.SelectedIndex = 0;
            ckbUseCC.Enabled = ObjectCloner.Properties.Settings.Default.CCEnabled;
            ckbUseCC.Checked = ckbUseCC.Enabled && FileTable.UseCustomContent;
        }

        public Search(MainForm.updateProgressCallback updateProgressCB, MainForm.listViewAddCallBack listViewAddCB)
            : this()
        {
            this.updateProgressCB = updateProgressCB;
            this.listViewAddCB = listViewAddCB;
        }



        void Search_LoadListViewSettings()
        {
            string cw = ObjectCloner.Properties.Settings.Default.ColumnWidths;
            string[] cws = cw == null ? new string[] { } : cw.Split(':');

            int w;
            listView1.Columns[0].Width = cws.Length > 0 && int.TryParse(cws[0], out w) && w > 0 ? w : (this.listView1.Width - 32) / 3;
            listView1.Columns[1].Width = cws.Length > 1 && int.TryParse(cws[1], out w) && w > 0 ? w : (this.listView1.Width - 32) / 6;
            listView1.Columns[2].Width = cws.Length > 2 && int.TryParse(cws[2], out w) && w > 0 ? w : (this.listView1.Width - 32) / 6;
            listView1.Columns[3].Width = cws.Length > 3 && int.TryParse(cws[3], out w) && w > 0 ? w : (this.listView1.Width - 32) / 3;/**/
        }



        public ListViewItem SelectedItem { get { return listView1.SelectedItems.Count == 1 ? listView1.SelectedItems[0] : null; } set { listView1.SelectedItems.Clear(); if (value != null && listView1.Items.Contains(value)) try { value.Selected = true; } catch { } } }

        #region Occurs whenever the 'SelectedIndex' for the Search Results changes
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the 'SelectedIndex' for the Search Results changes")]
        public event EventHandler<MainForm.SelectedIndexChangedEventArgs> SelectedIndexChanged;
        protected virtual void OnSelectedIndexChanged(object sender, MainForm.SelectedIndexChangedEventArgs e) { if (SelectedIndexChanged != null) SelectedIndexChanged(sender, e); }
        #endregion

        #region Occurs when an item is actived
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs when an item is actived")]
        public event EventHandler<MainForm.ItemActivateEventArgs> ItemActivate;
        protected virtual void OnItemActivate(object sender, MainForm.ItemActivateEventArgs e) { if (ItemActivate != null) ItemActivate(sender, e); }
        #endregion

        private void listView1_SelectedIndexChanged(object sender, EventArgs e) { OnSelectedIndexChanged(sender, new MainForm.SelectedIndexChangedEventArgs(sender as ListView)); }
        private void listView1_ItemActivate(object sender, EventArgs e) { OnItemActivate(sender, new MainForm.ItemActivateEventArgs(sender as ListView)); }

        private void scmCopyRK_Click(object sender, EventArgs e)
        {
            if (SelectedItem != null && SelectedItem.Tag as SpecificResource != null)
                Clipboard.SetText((SelectedItem.Tag as SpecificResource).ResourceIndexEntry + "");
        }



        private void searchContextMenu_Opening(object sender, CancelEventArgs e)
        {
            scmCopyRK.Enabled = SelectedItem != null;
        }

        #region Search Controls
        private void ckb_CheckedChanged(object sender, EventArgs e)
        {
            btnSearch.Enabled = haveCriteria();
        }

        private void tbText_TextChanged(object sender, EventArgs e)
        {
            btnSearch.Enabled = haveCriteria();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (searching)
                AbortSearch(false);
            else
            {
                tlpSearch.Visible = false;
                MainForm.SetUseCC(ckbUseCC.Checked);
                if (!MainForm.CheckInstallDirs(this))
                    return;
                tlpSearch.Visible = true;

                listView1.Enabled = searchContextMenu.Enabled = tbText.Enabled = tlpWhere.Enabled = cbCatalogType.Enabled = false;
                btnSearch.Text = "&Stop";
                StartSearch();
            }
        }

        public event EventHandler CancelClicked;
        private void btnCancel_Click(object sender, EventArgs e) { if (CancelClicked != null) CancelClicked(this, EventArgs.Empty); }

        public CatalogType SelectedCatalogType { get { return cbCatalogType.SelectedIndex > 0 ? ((CatalogType[])Enum.GetValues(typeof(CatalogType)))[cbCatalogType.SelectedIndex - 1] : 0; } }

        bool haveCriteria()
        {
            return tbText.Text.Length > 0 &&
                (ckbResourceName.Checked || ckbCatalogDesc.Checked || ckbCatalogName.Checked || ckbObjectDesc.Checked || ckbObjectName.Checked);
        }
        #endregion

        #region ListViewColumnSorter
        /// <summary>
        /// This class is an implementation of the 'IComparer' interface.
        /// </summary>
        public class ListViewColumnSorter : IComparer
        {
            /// <summary>
            /// Specifies the column to be sorted
            /// </summary>
            private int ColumnToSort;
            /// <summary>
            /// Specifies the order in which to sort (i.e. 'Ascending').
            /// </summary>
            private SortOrder OrderOfSort;
            /// <summary>
            /// Case insensitive comparer object
            /// </summary>
            private CaseInsensitiveComparer ObjectCompare;

            /// <summary>
            /// Class constructor.  Initializes various elements
            /// </summary>
            public ListViewColumnSorter()
            {
                // Initialize the CaseInsensitiveComparer object
                ObjectCompare = new CaseInsensitiveComparer();

                ListViewColumnSorter_LoadSettings();
            }

            void ListViewColumnSorter_LoadSettings()
            {
                ColumnToSort = ObjectCloner.Properties.Settings.Default.ColumnToSort;

                OrderOfSort = Enum.IsDefined(typeof(SortOrder), ObjectCloner.Properties.Settings.Default.SortOrder)
                    ? (SortOrder)ObjectCloner.Properties.Settings.Default.SortOrder
                    : SortOrder.None;
            }

            public void ListViewColumnSorter_SaveSettings()
            {
                ObjectCloner.Properties.Settings.Default.ColumnToSort = ColumnToSort;
                ObjectCloner.Properties.Settings.Default.SortOrder = (int)OrderOfSort;
            }

            /// <summary>
            /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
            /// </summary>
            /// <param name="x">First object to be compared</param>
            /// <param name="y">Second object to be compared</param>
            /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
            public int Compare(object x, object y)
            {
                if (ColumnToSort < 0 || ColumnToSort > ((ListViewItem)x).SubItems.Count) return 0;

                int compareResult = 0;
                ListViewItem listviewX, listviewY;

                // Cast the objects to be compared to ListViewItem objects
                listviewX = (ListViewItem)x;
                listviewY = (ListViewItem)y;

                // Compare the two items
                if (ColumnToSort == 3)
                    compareResult = (listviewX.Tag as SpecificResource).RequestedRK.CompareTo((listviewY.Tag as SpecificResource).RequestedRK);
                else
                    compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);

                // Calculate correct return value based on object comparison
                if (OrderOfSort == SortOrder.Ascending)
                {
                    // Ascending sort is selected, return normal result of compare operation
                    return compareResult;
                }
                else if (OrderOfSort == SortOrder.Descending)
                {
                    // Descending sort is selected, return negative result of compare operation
                    return (-compareResult);
                }
                else
                {
                    // Return '0' to indicate they are equal
                    return 0;
                }
            }

            /// <summary>
            /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
            /// </summary>
            public int SortColumn
            {
                set
                {
                    ColumnToSort = value;
                }
                get
                {
                    return ColumnToSort;
                }
            }

            /// <summary>
            /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
            /// </summary>
            public SortOrder Order
            {
                set
                {
                    OrderOfSort = value;
                }
                get
                {
                    return OrderOfSort;
                }
            }

        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.listView1.Sort();

            if (listView1.SelectedIndices.Count > 0)
                listView1.SelectedItems[0].EnsureVisible();
        }
        #endregion

        #region Search thread
        bool searching;
        Thread searchThread;

        void StartSearch()
        {
            Diagnostics.Log("StartSearch");

            listView1.Items.Clear();

            SearchThread st = new SearchThread(this, Add, updateProgressCB, stopSearch, OnSearchComplete);
            SearchComplete += new EventHandler<MainForm.BoolEventArgs>(Search_SearchComplete);

            searchThread = new Thread(new ThreadStart(st.Search));
            searching = true;
            searchThread.Start();
        }

        void Search_SearchComplete(object sender, MainForm.BoolEventArgs e)
        {
            Diagnostics.Log(String.Format("Search_SearchComplete {0}", e.arg));
            searching = false;
            while (searchThread != null && searchThread.IsAlive)
                searchThread.Join(100);
            searchThread = null;

            updateProgressCB(true, "", true, -1, false, 0);

            listView1.Enabled = searchContextMenu.Enabled = tbText.Enabled = tlpWhere.Enabled = cbCatalogType.Enabled = true;
            btnSearch.Text = "&Search";
            
            
            
            if (listView1.Items.Count > 0) { SelectedItem = listView1.Items[0]; SelectedItem.EnsureVisible(); listView1.Focus(); }


        }

        public void AbortSearch(bool abort)
        {
            if (abort)
            {
                if (searchThread != null) searchThread.Abort();
                searching = false;
                while (searchThread != null && searchThread.IsAlive)
                    searchThread.Join(100);
                searchThread = null;
            }
            else
            {
                if (!searching) Search_SearchComplete(null, new MainForm.BoolEventArgs(false));
                else searching = false;
            }
        }

        delegate void AddCallBack(SpecificResource item);
        void Add(SpecificResource item)
        {
            if (!this.IsHandleCreated || this.IsDisposed) return;
            listViewAddCB(item, listView1);
        }

        event EventHandler<MainForm.BoolEventArgs> SearchComplete;
        delegate void searchCompleteCallback(bool complete);
        void OnSearchComplete(bool complete) { if (SearchComplete != null) { SearchComplete(this, new MainForm.BoolEventArgs(complete)); } }

        public delegate bool stopSearchCallback();
        private bool stopSearch() { return !searching; }

        class SearchThread
        {
            struct Criteria
            {
                public string text;
                public bool resourceName;
                public bool objectName;
                public bool objectDesc;
                public bool catalogName;
                public bool catalogDesc;
                public bool allLanguages; // false for English only
                public CatalogType catalogType;
            }
            Criteria criteria;
            Control control;
            AddCallBack addCB;
            MainForm.updateProgressCallback updateProgressCB;
            stopSearchCallback stopSearchCB;
            searchCompleteCallback searchCompleteCB;

            IDictionary<ulong, string> nameMap = null;
            List<IDictionary<ulong, string>> stbls = null;

            public SearchThread(Search searchPane,
                AddCallBack addCB, MainForm.updateProgressCallback updateProgressCB, stopSearchCallback stopSearchCB, searchCompleteCallback searchCompleteCB)
            {
                this.control = searchPane;
                this.criteria.text = searchPane.tbText.Text.Trim().ToLowerInvariant();
                this.criteria.resourceName = searchPane.ckbResourceName.Checked;
                this.criteria.objectName = searchPane.ckbObjectName.Checked;
                this.criteria.objectDesc = searchPane.ckbObjectDesc.Checked;
                this.criteria.catalogName = searchPane.ckbCatalogName.Checked;
                this.criteria.catalogDesc = searchPane.ckbCatalogDesc.Checked;
                this.criteria.allLanguages = !searchPane.rb1English.Checked;
                this.criteria.allLanguages = searchPane.rb1All.Checked;
                this.criteria.catalogType = searchPane.SelectedCatalogType;
                this.addCB = addCB;
                this.updateProgressCB = updateProgressCB;
                this.stopSearchCB = stopSearchCB;
                this.searchCompleteCB = searchCompleteCB;
            }

            public void Search()
            {
                bool complete = false;
                bool abort = false;
                List<SpecificResource> lres = new List<SpecificResource>();
                try
                {
                    updateProgress(true, "Searching...", true, FileTable.fb0.Count, true, 0);
                    for (int p = 0; p < FileTable.fb0.Count; p++)
                    {
                        if (stopSearch) return;
                        PathPackageTuple ppt = FileTable.fb0[p];

                        #region Fetch name map
                        if (criteria.resourceName)
                        {
                            updateProgress(true, String.Format("Retrieving name map for package {0} of {1}...", p + 1, FileTable.fb0.Count), false, -1, false, -1);
                            SpecificResource sr = ppt.Find(rie => rie.ResourceType == 0x0166038C);
                            nameMap = sr == null ? null : sr.Resource as IDictionary<ulong, string>;
                            if (stopSearch) return;
                        }
                        #endregion

                        #region Fetch STBLs
                        if (criteria.catalogName || criteria.catalogDesc)
                        {
                            updateProgress(true, String.Format("Retrieving string tables for package {0} of {1}...", p + 1, FileTable.fb0.Count), true, -1, false, 0);
                            stbls = ppt.FindAll(rie => rie.ResourceType == 0x220557DA && (criteria.allLanguages || rie.Instance >> 56 == 0x00))
                                .ConvertAll<IDictionary<ulong, string>>(sr => sr.Resource as IDictionary<ulong, string>);
                            if (stopSearch) return;
                        }
                        #endregion

                        // Find the right type of resource to search
                        updateProgress(true, String.Format("Retrieving resource list {0} of {1}...", p + 1, FileTable.fb0.Count), false, -1, false, -1);
                        List<SpecificResource> lsr = Find(ppt);
                        if (stopSearch) return;

                        // Find the matches
                        updateProgress(true, String.Format("Finding matching resources {0} of {1}...", p + 1, FileTable.fb0.Count), false, -1, false, -1);
                        List<SpecificResource> matches = lsr.FindAll(Match);
                        if (stopSearch) return;

                        // Avoid duplicates
                        updateProgress(true, String.Format("De-duplicating matches {0} of {1}...", p + 1, FileTable.fb0.Count), false, -1, false, -1);
                        lres = matches.FindAll(match =>
                            {
                                if (stopSearch) return false;
                                if (lres.Exists(x => match.ResourceIndexEntry.Instance == x.ResourceIndexEntry.Instance)) return false;
                                Add(match);
                                return true;
                            }
                        );

                        updateProgress(false, "", false, -1, true, p + 1);
                    }
                    complete = true;
                }
                catch (ThreadAbortException) { abort = true; }
                finally
                {
                    if (!abort)
                    {
                        updateProgress(true, "Search ended", true, -1, false, 0);
                        searchComplete(complete);
                    }
                }
            }

            #region Search criteria matching
            List<SpecificResource> Find(PathPackageTuple ppt)
            {
                if (criteria.catalogType == 0)
                    return ppt.FindAll(rie => Enum.IsDefined(typeof(CatalogType), rie.ResourceType));
                else
                    return ppt.FindAll(rie => criteria.catalogType == (CatalogType)rie.ResourceType);
            }

            bool Match(SpecificResource match)
            {
                match = match.ResourceIndexEntry.ResourceType == (uint)CatalogType.ModularResource ? MainForm.ItemForTGIBlock0(match) : match;
                if (criteria.resourceName && MatchResourceName(match)) return true;
                if (criteria.objectName && MatchObjectName(match)) return true;
                if (criteria.objectDesc && MatchObjectDesc(match)) return true;
                if (criteria.catalogName && MatchCatalogName(match)) return true;
                if (criteria.catalogDesc && MatchCatalogDesc(match)) return true;
                return false;
            }
            bool MatchResourceName(SpecificResource item) { return nameMap != null && nameMap.ContainsKey(item.ResourceIndexEntry.Instance) && nameMap[item.ResourceIndexEntry.Instance].Trim().ToLowerInvariant().Contains(criteria.text); }
            bool MatchObjectName(SpecificResource item) { return ((string)item.Resource["CommonBlock.Name"].Value).Trim().ToLowerInvariant().Contains(criteria.text); }
            bool MatchObjectDesc(SpecificResource item) { return ((string)item.Resource["CommonBlock.Desc"].Value).Trim().ToLowerInvariant().Contains(criteria.text); }
            bool MatchCatalogName(SpecificResource item) { return MatchStbl((ulong)item.Resource["CommonBlock.NameGUID"].Value); }
            bool MatchCatalogDesc(SpecificResource item) { return MatchStbl((ulong)item.Resource["CommonBlock.DescGUID"].Value); }
            bool MatchStbl(ulong guid) { foreach (var stbl in stbls) if (stbl.ContainsKey(guid) && stbl[guid].Trim().ToLowerInvariant().Contains(criteria.text)) return true; return false; }
            #endregion

            #region Callbacks
            void Add(SpecificResource sr) { Thread.Sleep(0); if (control.IsHandleCreated && !control.IsDisposed) control.Invoke(addCB, new object[] { sr, }); }

            void updateProgress(bool changeText, string text, bool changeMax, int max, bool changeValue, int value) { Thread.Sleep(0); if (control.IsHandleCreated && !control.IsDisposed) control.Invoke(updateProgressCB, new object[] { changeText, text, changeMax, max, changeValue, value, }); }

            bool stopSearch { get { Thread.Sleep(0); return !(control.IsHandleCreated && !control.IsDisposed) || (bool)control.Invoke(stopSearchCB); } }

            void searchComplete(bool complete) { Thread.Sleep(0); if (control.IsHandleCreated && !control.IsDisposed) control.BeginInvoke(searchCompleteCB, new object[] { complete, }); }
            #endregion
        }
        #endregion
    }
}