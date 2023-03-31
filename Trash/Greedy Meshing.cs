    private void PrivateUpdateChunk()
    {
        _threadLocked = true;
        ushort[,,] voxels = new ushort[VoxelData.ChunkWidthInVoxels, VoxelData.ChunkHeightInVoxels, VoxelData.ChunkWidthInVoxels];
        byte[,,] greedyX = new byte[VoxelData.ChunkWidthInVoxels, VoxelData.ChunkHeightInVoxels, VoxelData.ChunkWidthInVoxels];
        byte[,,] greedyY = new byte[VoxelData.ChunkWidthInVoxels, VoxelData.ChunkHeightInVoxels, VoxelData.ChunkWidthInVoxels];
        while (Modifications.Count > 0)
        {
            VoxelMod v = Modifications.Dequeue();
            Vector3 position = v.Position -= Position;
            VoxelMap[(int)position.x, (int)position.y, (int)position.z] = v.ID;
        }

        ClearMeshData();
        #region Greedy Meshing Checks
        //back
        for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                                        if (WorldObj.VoxelTypes[VoxelMap[x, y, z]].IsSolid)
                                            UpdateMeshData(new Vector3(x * VoxelSize, y * VoxelSize, z * VoxelSize));
                    if (CheckVoxelIsTransparent(new Vector3(z, y, x) + VoxelData.FaceCheck[0]) != CheckVoxelIsTransparent(new Vector3(z, y, x)) || CheckVoxelIsSolid(new Vector3(z, y, x) + VoxelData.FaceCheck[0]) != CheckVoxelIsSolid(new Vector3(z, y, x)))
                        voxels[z, y, x] = VoxelMap[z, y, x];
                    else
                        voxels[z, y, x] = 0;
                    greedyX[z, y, x] = 1;
                    greedyY[z, y, x] = 1;
                }
            }
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 1; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (voxels[z, y, x] != 0 && voxels[z, y, x] == voxels[z - 1, y, x])
                    {
                        greedyX[z, y, x] += greedyX[z - 1, y, x];
                        greedyX[z - 1, y, x] = 0;
                        greedyY[z - 1, y, x] = 0;
                    }
                }
            }
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 1; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (voxels[y, z, x] != 0 && greedyX[y, z, x] != 0 && voxels[y, z, x] == voxels[y, z - 1, x] && greedyX[y, z, x] == greedyX[y, z - 1, x])
                    {
                        greedyY[y, z, x] += greedyY[y, z - 1, x];
                        greedyX[y, z - 1, x] = 0;
                        greedyY[y, z - 1, x] = 0;
                    }
                }
            }
        }
        for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (greedyX[x, y, z] != 0 && voxels[x, y, z] != 0)
                    {
                        UpdateQuad0(new Vector3(x, y, z), greedyX[x, y, z], greedyY[x, y, z], voxels[x, y, z]);
                    }
                }
            }
        }
        //front
        for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (CheckVoxelIsTransparent(new Vector3(z, y, x) + VoxelData.FaceCheck[1]) != CheckVoxelIsTransparent(new Vector3(z, y, x)) || CheckVoxelIsSolid(new Vector3(z, y, x) + VoxelData.FaceCheck[1]) != CheckVoxelIsSolid(new Vector3(z, y, x)))
                        voxels[z, y, x] = VoxelMap[z, y, x];
                    else
                        voxels[z, y, x] = 0;
                    greedyX[z, y, x] = 1;
                    greedyY[z, y, x] = 1;
                }
            }
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 1; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (voxels[z, y, x] != 0 && voxels[z, y, x] == voxels[z - 1, y, x])
                    {
                        greedyX[z, y, x] += greedyX[z - 1, y, x];
                        greedyX[z - 1, y, x] = 0;
                        greedyY[z - 1, y, x] = 0;
                    }
                }
            }
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 1; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (voxels[y, z, x] != 0 && greedyX[y, z, x] != 0 && voxels[y, z, x] == voxels[y, z - 1, x] && greedyX[y, z, x] == greedyX[y, z - 1, x])
                    {
                        greedyY[y, z, x] += greedyY[y, z - 1, x];
                        greedyX[y, z - 1, x] = 0;
                        greedyY[y, z - 1, x] = 0;
                    }
                }
            }
        }
        for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (greedyX[x, y, z] != 0 && voxels[x, y, z] != 0)
                    {
                        UpdateQuad1(new Vector3(x, y, z), greedyX[x, y, z], greedyY[x, y, z], voxels[x, y, z]);
                    }
                }
            }
        }
        //bottom
        for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (CheckVoxelIsTransparent(new Vector3(z, x, y) + VoxelData.FaceCheck[2]) != CheckVoxelIsTransparent(new Vector3(z,x,y)) || CheckVoxelIsSolid(new Vector3(z, x, y) + VoxelData.FaceCheck[2]) != CheckVoxelIsSolid(new Vector3(z, x, y)))
                        voxels[z, x, y] = VoxelMap[z, x, y];
                    else
                        voxels[z, x, y] = 0;
                    greedyX[z, x, y] = 1;
                    greedyY[z, x, y] = 1;
                }
            }
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 1; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (voxels[z, x, y] != 0 && voxels[z, x, y] == voxels[z - 1, x, y])
                    {
                        greedyX[z, x, y] += greedyX[z - 1, x, y];
                        greedyX[z - 1, x, y] = 0;
                        greedyY[z - 1, x, y] = 0;
                    }
                }
            }
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 1; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (voxels[y, x, z] != 0 && greedyX[y, x, z] != 0 && voxels[y, x, z] == voxels[y, x, z - 1] && greedyX[y, x, z] == greedyX[y, x, z - 1])
                    {
                        greedyY[y, x, z] += greedyY[y, x, z - 1];
                        greedyX[y, x, z - 1] = 0;
                        greedyY[y, x, z - 1] = 0;
                    }
                }
            }
        }
        for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (greedyX[x, y, z] != 0 && voxels[x, y, z] != 0)
                    {
                        UpdateQuad2(new Vector3(x, y, z), greedyX[x, y, z], greedyY[x, y, z], voxels[x, y, z]);
                    }
                }
            }
        }
        //top
        for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (CheckVoxelIsTransparent(new Vector3(z, x, y) + VoxelData.FaceCheck[3]) != CheckVoxelIsTransparent(new Vector3(z, x, y)) || CheckVoxelIsSolid(new Vector3(z, x, y) + VoxelData.FaceCheck[3]) != CheckVoxelIsSolid(new Vector3(z, x, y)))
                        voxels[z, x, y] = VoxelMap[z, x, y];
                    else
                        voxels[z, x, y] = 0;
                    greedyX[z, x, y] = 1;
                    greedyY[z, x, y] = 1;
                }
            }
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 1; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (voxels[z, x, y] != 0 && voxels[z, x, y] == voxels[z - 1, x, y])
                    {
                        greedyX[z, x, y] += greedyX[z - 1, x, y];
                        greedyX[z - 1, x, y] = 0;
                        greedyY[z - 1, x, y] = 0;
                    }
                }
            }
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 1; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (voxels[y, x, z] != 0 && greedyX[y, x, z] != 0 && voxels[y, x, z] == voxels[y, x, z - 1] && greedyX[y, x, z] == greedyX[y, x, z - 1])
                    {
                        greedyY[y, x, z] += greedyY[y, x, z - 1];
                        greedyX[y, x, z - 1] = 0;
                        greedyY[y, x, z - 1] = 0;
                    }
                }
            }
        }
        for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (greedyX[x, y, z] != 0 && voxels[x, y, z] != 0)
                    {
                        UpdateQuad3(new Vector3(x, y, z), greedyX[x, y, z], greedyY[x, y, z], voxels[x, y, z]);
                    }
                }
            }
        }
        //left
        for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (CheckVoxelIsTransparent(new Vector3(x, y, z) + VoxelData.FaceCheck[4]) != CheckVoxelIsTransparent(new Vector3(x, y, z)) || CheckVoxelIsSolid(new Vector3(x, y, z) + VoxelData.FaceCheck[4]) != CheckVoxelIsSolid(new Vector3(x, y, z)))
                        voxels[x, y, z] = VoxelMap[x, y, z];
                    else
                        voxels[x, y, z] = 0;
                    greedyX[x, y, z] = 1;
                    greedyY[x, y, z] = 1;
                }
            }
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 1; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (voxels[x, y, z] != 0 && voxels[x, y, z] == voxels[x, y, z - 1])
                    {
                        greedyX[x, y, z] += greedyX[x, y, z - 1];
                        greedyX[x, y, z - 1] = 0;
                        greedyY[x, y, z - 1] = 0;
                    }
                }
            }
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 1; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (voxels[x, z, y] != 0 && greedyX[x, z, y] != 0 && voxels[x, z, y] == voxels[x, z - 1, y] && greedyX[x, z, y] == greedyX[x, z - 1, z])
                    {
                        greedyY[x, z, y] += greedyY[x, z - 1, y];
                        greedyX[x, z - 1, y] = 0;
                        greedyY[x, z - 1, y] = 0;
                    }
                }
            }
        }
        for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (greedyX[x, y, z] != 0 && voxels[x, y, z] != 0)
                    {
                        UpdateQuad4(new Vector3(x, y, z), greedyX[x, y, z], greedyY[x, y, z], voxels[x, y, z]);
                    }
                }
            }
        }
        //right
        for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (CheckVoxelIsTransparent(new Vector3(x, y, z) + VoxelData.FaceCheck[5]) != CheckVoxelIsTransparent(new Vector3(x, y, z)) || CheckVoxelIsSolid(new Vector3(x, y, z) + VoxelData.FaceCheck[5]) != CheckVoxelIsSolid(new Vector3(x, y, z)))
                        voxels[x, y, z] = VoxelMap[x, y, z];
                    else
                        voxels[x, y, z] = 0;
                    greedyX[x, y, z] = 1;
                    greedyY[x, y, z] = 1;
                }
            }
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 1; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (voxels[x, y, z] != 0 && voxels[x, y, z] == voxels[x, y, z - 1])
                    {
                        greedyX[x, y, z] += greedyX[x, y, z - 1];
                        greedyX[x, y, z - 1] = 0;
                        greedyY[x, y, z - 1] = 0;
                    }
                }
            }
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 1; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (voxels[x, z, y] != 0 && greedyX[x, z, y] != 0 && voxels[x, z, y] == voxels[x, z - 1, y] && greedyX[x, z, y] == greedyX[x, z - 1, z])
                    {
                        greedyY[x, z, y] += greedyY[x, z - 1, y];
                        greedyX[x, z - 1, y] = 0;
                        greedyY[x, z - 1, y] = 0;
                    }
                }
            }
        }
        for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (greedyX[x, y, z] != 0 && voxels[x, y, z] != 0)
                    {
                        UpdateQuad5(new Vector3(x, y, z), greedyX[x, y, z], greedyY[x, y, z], voxels[x, y, z]);
                    }
                }
            }
        }
        #endregion
        lock (WorldObj.ChunksToDraw)
        {
            WorldObj.ChunksToDraw.Enqueue(this);
        }

        _threadLocked = false;
    }
        #region Greedy Meshing Quads
        // back face (i think)
        void UpdateQuad0(Vector3 pos, byte x, byte y, ushort voxelID)
        {
            bool isTransparent = WorldObj.VoxelTypes[voxelID].IsTransparent;
            _vertices.Add(pos + new Vector3(1 - x, 1 - y, 0));
            _vertices.Add(pos + new Vector3(1 - x, 1, 0));
            _vertices.Add(pos + new Vector3(1, 1 - y, 0));
            _vertices.Add(pos + new Vector3(1, 1, 0));
    
            AddTexture(WorldObj.VoxelTypes[voxelID].GetTextureID(0));
            if (!isTransparent)
            {
                _triangles.Add(_vertexIndex);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 3);
            }
            else
            {
                _transparentTriangles.Add(_vertexIndex);
                _transparentTriangles.Add(_vertexIndex + 1);
                _transparentTriangles.Add(_vertexIndex + 2);
                _transparentTriangles.Add(_vertexIndex + 2);
                _transparentTriangles.Add(_vertexIndex + 1);
                _transparentTriangles.Add(_vertexIndex + 3);
            }
            _vertexIndex += 4;
        }
    
        // front face (i think)
        void UpdateQuad1(Vector3 pos, byte x, byte y, ushort voxelID)
        {
            bool isTransparent = WorldObj.VoxelTypes[voxelID].IsTransparent;
            _vertices.Add(pos + new Vector3(1, 1 - y, 1));
            _vertices.Add(pos + new Vector3(1, 1, 1));
            _vertices.Add(pos + new Vector3(1 - x, 1 - y, 1));
            _vertices.Add(pos + new Vector3(1 - x, 1, 1));
    
            AddTexture(WorldObj.VoxelTypes[voxelID].GetTextureID(1));
            if (!isTransparent)
            {
                _triangles.Add(_vertexIndex);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 3);
            }
            else
            {
                _transparentTriangles.Add(_vertexIndex);
                _transparentTriangles.Add(_vertexIndex + 1);
                _transparentTriangles.Add(_vertexIndex + 2);
                _transparentTriangles.Add(_vertexIndex + 2);
                _transparentTriangles.Add(_vertexIndex + 1);
                _transparentTriangles.Add(_vertexIndex + 3);
            }
            _vertexIndex += 4;
        }
        // bottom face (i think)
        void UpdateQuad2(Vector3 pos, byte x, byte y, ushort voxelID)
        {
            bool isTransparent = WorldObj.VoxelTypes[voxelID].IsTransparent;
            _vertices.Add(pos + new Vector3(1, 0, 1 - y));
            _vertices.Add(pos + new Vector3(1, 0, 1));
            _vertices.Add(pos + new Vector3(1 - x, 0, 1 - y));
            _vertices.Add(pos + new Vector3(1 - x, 0, 1));
    
            AddTexture(WorldObj.VoxelTypes[voxelID].GetTextureID(2));
            if (!isTransparent)
            {
                _triangles.Add(_vertexIndex);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 3);
            }
            else
            {
                _transparentTriangles.Add(_vertexIndex);
                _transparentTriangles.Add(_vertexIndex + 1);
                _transparentTriangles.Add(_vertexIndex + 2);
                _transparentTriangles.Add(_vertexIndex + 2);
                _transparentTriangles.Add(_vertexIndex + 1);
                _transparentTriangles.Add(_vertexIndex + 3);
            }
            _vertexIndex += 4;
        }
        // top face (i think)
        void UpdateQuad3(Vector3 pos, byte x, byte y, ushort voxelID)
        {
            bool isTransparent = WorldObj.VoxelTypes[voxelID].IsTransparent;
            _vertices.Add(pos + new Vector3(1 - x, 1, 1 - y));
            _vertices.Add(pos + new Vector3(1 - x, 1, 1));
            _vertices.Add(pos + new Vector3(1, 1, 1 - y));
            _vertices.Add(pos + new Vector3(1, 1, 1));
    
            AddTexture(WorldObj.VoxelTypes[voxelID].GetTextureID(3));
            if (!isTransparent)
            {
                _triangles.Add(_vertexIndex);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 3);
            }
            else
            {
                _transparentTriangles.Add(_vertexIndex);
                _transparentTriangles.Add(_vertexIndex + 1);
                _transparentTriangles.Add(_vertexIndex + 2);
                _transparentTriangles.Add(_vertexIndex + 2);
                _transparentTriangles.Add(_vertexIndex + 1);
                _transparentTriangles.Add(_vertexIndex + 3);
            }
            _vertexIndex += 4;
        }
        // left face (i think)
        void UpdateQuad4(Vector3 pos, byte x, byte y, ushort voxelID)
        {
            bool isTransparent = WorldObj.VoxelTypes[voxelID].IsTransparent;
            _vertices.Add(pos + new Vector3(0, 1 - y, 1));
            _vertices.Add(pos + new Vector3(0, 1, 1));
            _vertices.Add(pos + new Vector3(0, 1 - y, 1 - x));
            _vertices.Add(pos + new Vector3(0, 1, 1 - x));
    
            AddTexture(WorldObj.VoxelTypes[voxelID].GetTextureID(4));
            if (!isTransparent)
            {
                _triangles.Add(_vertexIndex);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 3);
            }
            else
            {
                _transparentTriangles.Add(_vertexIndex);
                _transparentTriangles.Add(_vertexIndex + 1);
                _transparentTriangles.Add(_vertexIndex + 2);
                _transparentTriangles.Add(_vertexIndex + 2);
                _transparentTriangles.Add(_vertexIndex + 1);
                _transparentTriangles.Add(_vertexIndex + 3);
            }
            _vertexIndex += 4;
        }
        // right face (i think)
        void UpdateQuad5(Vector3 pos, byte x, byte y, ushort voxelID)
        {
            bool isTransparent = WorldObj.VoxelTypes[voxelID].IsTransparent;
            _vertices.Add(pos + new Vector3(1, 1 - y, 1 - x));
            _vertices.Add(pos + new Vector3(1, 1, 1 - x));
            _vertices.Add(pos + new Vector3(1, 1 - y, 1));
            _vertices.Add(pos + new Vector3(1, 1, 1));
    
            AddTexture(WorldObj.VoxelTypes[voxelID].GetTextureID(5));
            if (!isTransparent)
            {
                _triangles.Add(_vertexIndex);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 3);
            }
            else
            {
                _transparentTriangles.Add(_vertexIndex);
                _transparentTriangles.Add(_vertexIndex + 1);
                _transparentTriangles.Add(_vertexIndex + 2);
                _transparentTriangles.Add(_vertexIndex + 2);
                _transparentTriangles.Add(_vertexIndex + 1);
                _transparentTriangles.Add(_vertexIndex + 3);
            }
            _vertexIndex += 4;
        }
        #endregion
