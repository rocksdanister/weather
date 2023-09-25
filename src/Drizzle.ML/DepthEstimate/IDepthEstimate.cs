using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.ML.DepthEstimate;

public interface IDepthEstimate : IDisposable
{
    void LoadModel(string path);
    ModelOutput Run(string imagePath);
    string ModelPath { get; }
}
