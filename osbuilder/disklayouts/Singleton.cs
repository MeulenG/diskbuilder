using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace OSBuilder.DiskLayouts {
    public class Singleton : IDiskScheme {
    private IDisk _disk = null;
    private List<FileSystems.IFileSystem> _fileSystems = new List<FileSystems.IFileSystem>();

    private ulong _sectorsAllocated = 0;

    public bool Open(IDisk disk)
    {
        // Store disk
        _disk = disk;

        // ensure disk is open for read/write
        if (!_disk.IsOpen())
            return false;

        return true;
    }

    public void Dispose()
    {
        if (_disk == null)
            return;
        
        // dispose of filesystems
        foreach (var fs in _fileSystems)
        {
            fs.Dispose();
        }
        
        // cleanup
        _fileSystems.Clear();
        _disk = null;
    }
    

    public bool Create(IDisk disk) 
    {
        _disk = disk;

        // ensure disk is open for read/write
        if (!_disk.IsOpen())
            return false;

        return true;
    }

    public bool AddPartition(FileSystems.IFileSystem fileSystem, ulong sectorCount, string vbrImage, string reservedSectorsImage)
    {
        ulong partitionSize = GetFreeSectorCount();
        if (_disk == null || fileSystem == null)
            return false;

        if (_fileSystems.Count != 0)
        {
            return false;
        }
        _sectorsAllocated += partitionSize;
        // Initialize the file-system
        fileSystem.Initialize(_disk, 0, partitionSize, vbrImage, reservedSectorsImage);
        
        // Add sectors allocated
        _fileSystems.Add(fileSystem);
        return fileSystem.Format();
    }
    public ulong GetFreeSectorCount()
    {
        if (_disk == null)
            return 0;
        return _disk.SectorCount - _sectorsAllocated;
    }
    public IEnumerable<FileSystems.IFileSystem> GetFileSystems()
    {
        return _fileSystems;
    }
            
    }
}