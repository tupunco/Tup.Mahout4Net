
namespace Tup.Hadoop.Mapred
{
    public interface IOutputCollector<K, V>
    {
        void Collect(K paramK, V paramV);
    }
}
