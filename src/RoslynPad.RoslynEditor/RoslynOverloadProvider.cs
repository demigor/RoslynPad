using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.CodeCompletion;
using Microsoft.CodeAnalysis;
using RoslynPad.Roslyn;
using RoslynPad.Roslyn.SignatureHelp;
using RoslynPad.Utilities;

namespace RoslynPad.RoslynEditor
{
    internal sealed class RoslynOverloadProvider : NotificationObject, IOverloadProvider
    {
        private readonly SignatureHelpItems _signatureHelp;
        private readonly IList<SignatureHelpItem> _items;

        private int _selectedIndex;
        private SignatureHelpItem _item;
        private object _currentHeader;
        private object _currentContent;
        private string _currentIndexText;

        public RoslynOverloadProvider(SignatureHelpItems signatureHelp)
        {
            _signatureHelp = signatureHelp;
            _items = signatureHelp.Items;
            CreateSignatureHelp();
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (SetProperty(ref _selectedIndex, value))
                {
                    CreateSignatureHelp();
                }
            }
        }

        private void CreateSignatureHelp()
        {
            _item = _items[_selectedIndex];
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    ToTextBlock(_item.PrefixDisplayParts),
                }
            };
            var contentPanel = new StackPanel();
            var docText = _item.DocumentationFactory(CancellationToken.None).ToTextBlock();
            if (docText != null && docText.Inlines.Count > 0)
            {
                contentPanel.Children.Add(docText);
            }
            if (!_item.Parameters.IsDefault)
            {
                for (var index = 0; index < _item.Parameters.Length; index++)
                {
                    var param = _item.Parameters[index];
                    AddParameterSignatureHelp(index, param, headerPanel, contentPanel);
                }
            }
            headerPanel.Children.Add(ToTextBlock(_item.SuffixDisplayParts));
            CurrentHeader = headerPanel;
            CurrentContent = contentPanel;
        }

        private void AddParameterSignatureHelp(int index, SignatureHelpParameter param, Panel headerPanel, Panel contentPanel)
        {
            var isSelected = _signatureHelp.ArgumentIndex == index;
            headerPanel.Children.Add(ToTextBlock(param.DisplayParts, bold: isSelected));
            if (index != _item.Parameters.Length - 1)
            {
                headerPanel.Children.Add(ToTextBlock(_item.SeparatorDisplayParts));
            }
            if (isSelected)
            {
                var textBlock = param.DocumentationFactory(CancellationToken.None).ToTextBlock();
                if (textBlock != null && textBlock.Inlines.Count > 0)
                {
                    contentPanel.Children.Add(new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new TextBlock {Text = param.Name + ": ", FontWeight = FontWeights.Bold},
                            textBlock
                        }
                    });
                }
            }
        }

        private static TextBlock ToTextBlock(IEnumerable<SymbolDisplayPart> parts, bool bold = false)
        {
            if (parts == null) return new TextBlock();
            var textBlock = parts.ToTextBlock();
            if (bold)
            {
                textBlock.FontWeight = FontWeights.Bold;
            }
            return textBlock;
        }

        public int Count => _items.Count;

        // ReSharper disable once UnusedMember.Local
        public string CurrentIndexText
        {
            get { return _currentIndexText; }
            private set { SetProperty(ref _currentIndexText, value); }
        }

        public object CurrentHeader
        {
            get { return _currentHeader; }
            private set { SetProperty(ref _currentHeader, value); }
        }

        public object CurrentContent
        {
            get { return _currentContent; }
            private set { SetProperty(ref _currentContent, value); }
        }
    }
}