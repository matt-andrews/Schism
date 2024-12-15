namespace Schism.Lib.Core.Interfaces;
public interface IStringTranslationFeature
{
    /// <summary>
    /// Determines if the given string is translatable using this implementation
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    bool IsTranslatable(string str);

    /// <summary>
    /// Translates the given string if <see cref="IsTranslatable(string)"/> returns true
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    Task<string> Translate(string str);
}