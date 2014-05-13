//namespace PillarKata
//{
using System.Windows.Controls;

namespace PillarKata.ViewModels
    {
        public static class TreeViewExtensions
        {
            #region Class Members

            public static TreeViewItem ContainerFromItem(this TreeView treeView, object item)
            {
                var containerThatMightContainItem = (TreeViewItem) treeView.ItemContainerGenerator.ContainerFromItem(item);
                if (containerThatMightContainItem != null)
                    return containerThatMightContainItem;
                else
                    return ContainerFromItem(treeView.ItemContainerGenerator, treeView.Items, item);
            }

            public static object ItemFromContainer(this TreeView treeView, TreeViewItem container)
            {
                var itemThatMightBelongToContainer = (TreeViewItem) treeView.ItemContainerGenerator.ItemFromContainer(container);
                if (itemThatMightBelongToContainer != null)
                    return itemThatMightBelongToContainer;
                else
                    return ItemFromContainer(treeView.ItemContainerGenerator, treeView.Items, container);
            }

            private static TreeViewItem ContainerFromItem(ItemContainerGenerator parentItemContainerGenerator,
                                                          ItemCollection itemCollection,
                                                          object item)
            {
                foreach (object curChildItem in itemCollection)
                {
                    var parentContainer = (TreeViewItem) parentItemContainerGenerator.ContainerFromItem(curChildItem);
                    var containerThatMightContainItem = (TreeViewItem) parentContainer.ItemContainerGenerator.ContainerFromItem(item);
                    if (containerThatMightContainItem != null)
                        return containerThatMightContainItem;
                    TreeViewItem recursionResult = ContainerFromItem(parentContainer.ItemContainerGenerator, parentContainer.Items,
                                                                     item);
                    if (recursionResult != null)
                        return recursionResult;
                }
                return null;
            }

            private static object ItemFromContainer(ItemContainerGenerator parentItemContainerGenerator,
                                                    ItemCollection itemCollection,
                                                    TreeViewItem container)
            {
                foreach (object curChildItem in itemCollection)
                {
                    var parentContainer = (TreeViewItem) parentItemContainerGenerator.ContainerFromItem(curChildItem);
                    var itemThatMightBelongToContainer =
                        (TreeViewItem) parentContainer.ItemContainerGenerator.ItemFromContainer(container);
                    if (itemThatMightBelongToContainer != null)
                        return itemThatMightBelongToContainer;
                    var recursionResult =
                        ItemFromContainer(parentContainer.ItemContainerGenerator, parentContainer.Items, container) as TreeViewItem;
                    if (recursionResult != null)
                        return recursionResult;
                }
                return null;
            }

            #endregion
        }
    }
//}