namespace ThirdPartyService.ServiceImplementation.AdsService.DummyAds.BannerAds
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Patterns.MVP;
    using GameFoundation.Scripts.Patterns.MVP.Attribute;
    using GameFoundation.Scripts.Patterns.MVP.Implementation;
    using GameFoundation.Scripts.Patterns.MVP.Signals;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using UnityEngine;


    public class FakeBannerSplashModel
    {
        public readonly BannerPosition Position;

        public FakeBannerSplashModel(BannerPosition position)
        {
            this.Position = position;
        }
    }

    [View(nameof(FakeBannerSplashView))]
    public class FakeBannerSplashView : SplashView
    {
        public RectTransform BannerRect;
    }

    [Presenter(isSingleton: true)]
    public class FakeBannerSplashPresenter : SplashPresenter<FakeBannerSplashView, FakeBannerSplashModel>
    {

        public FakeBannerSplashPresenter(
            IViewFactory viewFactory,
            SignalBus    signalBus,
            UICanvas     uiCanvas
        ) : base(viewFactory, signalBus, uiCanvas) { }
        protected override void Bind()
        {
            base.Bind();
            this.SetPosition(this.model.Position);
        }
        public override void Close()
        {
            // base.Close();
            this.OnHide(false);
            this.signalBus.Fire(new HidePresenterSignal(this));
            this.OnBeforeHide();
            // this.view.Hide();
            this.OnAfterHide();
        }
        protected override UniTask OnBeforeShow()
        {
            this.OnShow(false);
            return base.OnBeforeShow();
        }

        public void SetPosition(BannerPosition position)
        {
            var anchoredPosition = position switch
            {
                BannerPosition.TopCenter    => new Vector2(0, Screen.height - this.view.BannerRect.sizeDelta.y),
                BannerPosition.BottomCenter => new Vector2(0, 0),
                BannerPosition.Centered     => new Vector2(0, (Screen.height - this.view.BannerRect.sizeDelta.y) / 2),
                _                           => Vector2.zero,
            };
            this.view.BannerRect.anchoredPosition = anchoredPosition;
        }
        public float GetHeight()
        {
            return this.view.BannerRect.sizeDelta.y;
        }
    }
}