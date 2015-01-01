namespace IPFilter
{
    /// <summary>
    /// Defines a provider for progress updates.
    /// </summary>
    /// <typeparam name="T">The type of progress update value.This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
    public interface IProgress<in T>
    {
        /// <summary>
        /// Reports a progress update.
        /// </summary>
        /// <param name="value">The value of the updated progress.</param>
        void Report(T value);
    }
}