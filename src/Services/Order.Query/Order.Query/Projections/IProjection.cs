using Order.Query.Events;

namespace Order.Query.Projections;

public interface IProjection<out TView, in T> where T : Event
    where TView : class
{
    string Id { get; }
    
    public bool CanUpdate(T @event);
    
    public void Apply(T @event);
    
    static abstract TView CreateView(T @event);
}