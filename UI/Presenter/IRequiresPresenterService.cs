using Framework.UI.Model;

namespace Framework.UI.Presenter
{
    /// <summary>
    /// Marker interface for presenter models that require a reference to their presenter service.
    /// Models implementing this interface will automatically receive the presenter reference
    /// during initialization in the base Presenter class.
    /// </summary>
    /// <typeparam name="TPresenterModel">The type of the presenter model.</typeparam>
    public interface IRequiresPresenterService<TPresenterModel> where TPresenterModel : IPresenterModel
    {
        /// <summary>
        /// Gets or sets the presenter service that owns this model.
        /// This property is automatically set by the base Presenter.Initialize() method.
        /// </summary>
        IPresenterService<TPresenterModel> Presenter { get; set; }
    }
}