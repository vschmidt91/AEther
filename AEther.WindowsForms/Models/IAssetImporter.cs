using System;
using System.Collections.Generic;
using System.Text;

namespace AEther.WindowsForms
{
    public interface IAssetImporter
    {

        T? Import<T>(string asset)
            where T : class;

    }
}
