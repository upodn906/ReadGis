namespace Reader.Abstraction.Objects.Default
{
    public class DefaultObjectProcessor : IGisObjectProcessor
    {
        public virtual Task ProcessAsync(IGisObject obj)
        {
            //if (obj.LayerStandardName == "Feeder" &&
            //    obj.Data.TryGetValue("FEEDER_NAME", out var fName))
            //{
            //    obj.MetaData ??= new Dictionary<string, object>();
            //    obj.MetaData.Add("Name" , fName);
            //}
            //else if (obj.LayerStandardName == "Sub_transmissionSubstations" &&
            //         obj.Data.TryGetValue("SU_NAME", out var name))
            //{
                
            //    obj.MetaData ??= new Dictionary<string, object>();
            //    obj.MetaData.Add("Name", name);
            //}
            return Task.CompletedTask;
        }

        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
