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
        public MainWindow()
        {
            InitializeComponent();
            ProductBoMRepoSql repo = new ProductBoMRepoSql();
            List<TreeBoMDTO> list = repo.getAllTree().ToList();
            ItemCollection allNodes = treeView.Items;

            var groupedList = from bomNode in list
                              orderby bomNode.level, bomNode.RequiredPartNo
                              group bomNode by bomNode.level;

            Dictionary<string, List<TreeViewItem>> itemChildCollection = new Dictionary<string, List<TreeViewItem>>();
            foreach (var partNoGroupItem in groupedList)
            {
                TreeViewItem item = new TreeViewItem();

                var itemsInner = item.Items;
                TreeViewItem itemChild = new TreeViewItem();
                foreach (var bomItem in partNoGroupItem)
                {
                    itemChild = new TreeViewItem();
                    itemChild.Tag = bomItem.level;
                    itemChild.Header = bomItem.RequiredPartNo;
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
                treeView.Items.Add(item);



            }//forEach






        }
    }
}
