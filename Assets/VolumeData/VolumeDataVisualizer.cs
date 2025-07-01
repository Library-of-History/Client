using Anaglyph.XRTemplate;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;
using System.Collections.Generic;
using Oculus.Interaction;

namespace MarchingCubes {

sealed class VolumeDataVisualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] private Vector3Int _dimensions;
    [SerializeField] float _gridScale = 0.1f;
    [SerializeField] int _triangleBudget = 65536 * 16;

    #endregion

    #region Project asset references

    [SerializeField] ComputeShader _builderCompute = null;

    #endregion

    #region Target isovalue

    [CreateProperty] public float TargetValue { get; set; } = 0.5f;
    float _builtTargetValue;

    #endregion

    #region Private members

    ComputeBuffer _voxelBuffer;
    MeshBuilder _builder;

    #endregion

    #region MonoBehaviour implementation

    void OnEnable()
    {
        _dimensions = new (EnvironmentMapper.Instance.Volume.width, 
            EnvironmentMapper.Instance.Volume.height, EnvironmentMapper.Instance.Volume.volumeDepth);
        _builder = new MeshBuilder(_dimensions, _triangleBudget, _builderCompute);
        _voxelBuffer = EnvironmentMapper.Instance.VoxelBuffer;
        
        _builder.BuildIsosurface(_voxelBuffer, TargetValue, _gridScale);
        GetComponent<MeshFilter>().sharedMesh = _builder.Mesh;

        // Voxel data conversion (ushort -> float)
        // using var readBuffer = new ComputeBuffer(VoxelCount / 2, sizeof(uint));

        // UI data source
        // FindFirstObjectByType<UIDocument>().rootVisualElement.dataSource = this;
    }

    void OnDestroy()
    {
        _voxelBuffer.Dispose();
        _builder.Dispose();
    }

    // void Update()
    // {
    //     // Rebuild the isosurface only when the target value has been changed.
    //     if (TargetValue == _builtTargetValue) return;
    //
    //     _builder.BuildIsosurface(_voxelBuffer, TargetValue, _gridScale);
    //     GetComponent<MeshFilter>().sharedMesh = _builder.Mesh;
    //
    //     _builtTargetValue = TargetValue;
    // }

    #endregion
}

} // namespace MarchingCubes
