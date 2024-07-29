using System.Collections.Generic;
using System.ComponentModel;

public class EventMonitor
{
    readonly TileTrackerClient tileTracker;
    readonly IMonoWrapper mono;

    public EventMonitor(ClassReferences refs)
    {
        tileTracker = refs.TileTrackerClient;
        tileTracker.PropertyChanged += UpdateRack;
        mono = refs.Mono;
    }

    void UpdateRack(object tileTrackerClient, PropertyChangedEventArgs e)
    {
        List<int> rackList = tileTrackerClient.GetType()
            .GetProperty(e.PropertyName)
            .GetValue(tileTrackerClient) as List<int>;

        //mono.UpdateRack(rackList);  
    }
}