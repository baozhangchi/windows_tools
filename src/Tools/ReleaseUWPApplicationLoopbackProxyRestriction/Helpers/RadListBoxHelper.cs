#region FileHeader

// // Project:  ReleaseUWPApplicationLoopbackProxyRestriction
// // File:  RadListBoxHelper.cs
// // CreateTime:  2023-01-03 9:04
// // LastUpdateTime:  2023-01-03 14:39

#endregion

#region Nmaespaces

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls;

#endregion

namespace ReleaseUWPApplicationLoopbackProxyRestriction.Helpers
{
    internal class RadListBoxHelper
    {
        // Using a DependencyProperty as the backing store for SelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached("SelectedItems", typeof(IList), typeof(RadListBoxHelper),
                new FrameworkPropertyMetadata(default, OnSelectedItemsPropertyChanged)
                {
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    BindsTwoWayByDefault = true
                });

        // Using a DependencyProperty as the backing store for IsAttach.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(RadListBoxHelper),
                new PropertyMetadata(false, OnAttachChanged));

        // Using a DependencyProperty as the backing store for IsUpdating.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(RadListBoxHelper),
                new PropertyMetadata(false));

        private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadListBox listBox)
            {
                if (!GetAttach(listBox))
                    listBox.SelectionChanged -= OnSelectionChanged;
                if (GetAttach(listBox))
                    listBox.SelectionChanged += OnSelectionChanged;
            }
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is RadListBox listBox)
            {
                SetIsUpdating(listBox, true);
                SetSelectedItems(listBox, listBox.SelectedItems);
                SetIsUpdating(listBox, false);
            }
        }

        public static IList GetSelectedItems(DependencyObject obj)
        {
            return (IList)obj.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(DependencyObject obj, IList value)
        {
            obj.SetValue(SelectedItemsProperty, value);
        }


        public static bool GetAttach(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttachProperty);
        }

        public static void SetAttach(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachProperty, value);
        }


        private static bool GetIsUpdating(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject obj, bool value)
        {
            obj.SetValue(IsUpdatingProperty, value);
        }


        private static void OnSelectedItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadListBox listBox)
            {
                listBox.SelectionChanged -= OnSelectionChanged;
                if (!GetIsUpdating(listBox))
                {
                    listBox.SelectedItems.Clear();
                    foreach (var selectedItem in GetSelectedItems(listBox))
                        listBox.SelectedItems.Add(selectedItem);
                }

                listBox.SelectionChanged += OnSelectionChanged;
            }
        }
    }
}