using Modules.RPG.Scripts.Adventure.Choice;
using Modules.Windows.Scripts.Base;
using System;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Choice
{
    /// <summary>
    /// Base ViewModel for a single adventure choice panel.
    /// Created via Zenject; call <see cref="Init"/> before use.
    /// </summary>
    public abstract class AdventureChoiceViewModelBase : ViewModelBase
    {
        private bool _isDisposed;

        public ChoiceData Data { get; private set; }

        public ChoiceType ChoiceType => Data.Type;

        public string Text { get; private set; }
        public string Description { get; private set; }
        public string MainIcon { get; private set; }
        public bool EnabledDescription { get; private set; }
        public string DescriptionIcon { get; private set; }

        public bool IsDisposed => _isDisposed;

        public virtual void Init(ChoiceData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));

            MainIcon = GetMainIcon();
            Text = GetText();
            Description = GetDescription();
            DescriptionIcon = GetDescriptionIcon();

            EnabledDescription = !string.IsNullOrEmpty(Description);
        }

        /// <summary>
        /// Player selected this choice. Subclasses execute actions / open dice check UI.
        /// </summary>
        public abstract void Select();

        public override void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            DisposeImplementation();
        }

        /// <summary>
        /// Override for subclass cleanup. Called once from <see cref="Dispose"/>.
        /// </summary>
        protected virtual void DisposeImplementation()
        {
        }

        private string GetMainIcon()
        {
            return Data.VisualOptions?.MainIcon ?? string.Empty;
        }

        private string GetText()
        {
            return Data.Text ?? string.Empty;
        }

        private string GetDescription()
        {
            return Data.Description ?? string.Empty;
        }

        private string GetDescriptionIcon()
        {
            return Data.VisualOptions?.DescriptionIcon ?? string.Empty;
        }
    }
}
