using PreactorRepositoryService.DAL.DTOModels;
using PreactorRepositoryService.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TreeViewSinara
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public tDataTemplateSelector newtDataTemplateSelector;

        public class tDataTemplateSelector : DataTemplateSelector
        {
            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                FrameworkElement element = container as FrameworkElement;
                return
                    element.FindResource("tSel") as DataTemplate;
            }
        }
        private void ExpandAllNodes(TreeViewItem treeItem)
        {
            treeItem.IsExpanded = true;
            foreach (var childItem in treeItem.Items.OfType<TreeViewItem>())
            {
                ExpandAllNodes(childItem);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            newtDataTemplateSelector = new tDataTemplateSelector();
            ProductBoMRepoSql repo = new ProductBoMRepoSql();
            List<TreeBoMDTO> list = repo.getAllTree().ToList();
            ItemCollection allNodes = treeView.Items;
            var groupedList = from bomNode in list
                              orderby bomNode.level, bomNode.RequiredPartNo
                              group bomNode by bomNode.level;

            Dictionary<string, List<TreeViewItem>> itemChildCollection = new Dictionary<string, List<TreeViewItem>>();
            treeView.ItemTemplateSelector = newtDataTemplateSelector;

            TreeViewItem item = new TreeViewItem();

            foreach (var partNoGroupItem in groupedList)
            {
                item.HeaderTemplate = FindResource("tSel") as DataTemplate;
                item.HeaderTemplateSelector = newtDataTemplateSelector;
                item.Header = new { PartNo = partNoGroupItem.Key, Tag = partNoGroupItem.Key };
                var itemsInner = item.Items;
                TreeViewItem itemChild = new TreeViewItem();
                itemChild.HeaderTemplate = FindResource("tSel") as DataTemplate;
                itemChild.HeaderTemplateSelector = newtDataTemplateSelector;
                foreach (var bomItem in partNoGroupItem)
                {
                    itemChild = new TreeViewItem();
                    int bomItemlevel = 0;
                    int.TryParse(bomItem.level, out bomItemlevel);
                    itemChild.Foreground = new SolidColorBrush(Color.FromRgb(220, (byte)(150*bomItemlevel), (byte)(160 * bomItemlevel)));
                    itemChild.HeaderTemplate = FindResource("tSel") as DataTemplate;
                    itemChild.HeaderTemplateSelector = newtDataTemplateSelector;
                    itemChild.Tag = bomItem.RequiredPartNo;
                    itemChild.Header = bomItem;

                    if (itemChildCollection.ContainsKey(bomItem.PartNo))
                    {
                        List<TreeViewItem> itemsInnerColl = itemChildCollection[bomItem.PartNo];
                        foreach (TreeViewItem el in itemsInnerColl)
                        {
                            itemsInner = el.Items;
                            itemsInner.Add(itemChild);
                            if (itemChildCollection.ContainsKey(bomItem.RequiredPartNo))
                                itemChildCollection[bomItem.RequiredPartNo].Add(itemChild);
                            else
                            {
                                itemChildCollection[bomItem.RequiredPartNo] = new List<TreeViewItem>();
                                itemChildCollection[bomItem.RequiredPartNo].Add(itemChild);

                            }
                        }



                    }
                    else
                    {
                        itemsInner.Add(itemChild);
                        if (itemChildCollection.ContainsKey(bomItem.RequiredPartNo))
                            itemChildCollection[bomItem.RequiredPartNo].Add(itemChild);
                        else
                        {
                            itemChildCollection[bomItem.RequiredPartNo] = new List<TreeViewItem>();
                            itemChildCollection[bomItem.RequiredPartNo].Add(itemChild);

                        }
                        itemsInner = itemChild.Items;

                    }

                }//foreach



            }//forEach
            treeView.Items.Add(item);
            treeView.Items.OfType<TreeViewItem>().ToList().ForEach(ExpandAllNodes);




        }
    }
}
