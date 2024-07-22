using System.Collections.Generic;
using System.ComponentModel;

public class EventMonitor
{
    readonly GameManagerClient GManagerClient;
    readonly IMonoWrapper mono;

    public EventMonitor(ClassReferences refs)
    {
        GManagerClient = refs.GManagerClient;
        GManagerClient.PropertyChanged += UpdateRack;
        mono = refs.Mono;
    }

    void UpdateRack(object gManagerClient, PropertyChangedEventArgs e)
    {
        List<int> rackList = gManagerClient.GetType()
            .GetProperty(e.PropertyName)
            .GetValue(gManagerClient) as List<int>;

        // TODO: finish this
        
    }
}
