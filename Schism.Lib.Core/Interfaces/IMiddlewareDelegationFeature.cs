namespace Schism.Lib.Core.Interfaces;

public interface IMiddlewareDelegationFeature
{
    Task<bool> Invoke();
}