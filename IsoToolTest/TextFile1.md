# Fraagment ISO Contents
<details>
<summary><strong>0x00008000 - ISO Volume Descriptors</strong></summary>

1. Primary Volume Descriptor `CD001`
2. Set Terminator Volume Descriptor `CD001`
3. `BEA01`
4. `NSR02`
5. `TEA01`

</details>  


<details>
<summary><strong>0x00010000 - UDF Volume Descriptors</strong></summary>

1. Primary Volume Descriptor
2. Implementation Use Volume Descriptor
3. Partition Descriptor
4. Logical Volume Descriptor
5. Unallocated Space Descriptor
6. Terminating Descriptor
</details>  

<strong>0x00018000 - Reserve UDF Volume Descriptors
  
0x00020000 - Logical Volume Integrity Descriptor

0x00020800 - Terminating Descriptor

0x00080000 - Anchor Volume Descriptor Pointer

0x00080800 - PathTables

0x00082800 - ISO Directories

0x00087800 - FileSetDescriptor (Start of Partition[0])

0x00088000 - TerminatingDescriptor

0x00088800 - Directory Entries

0x0008D800 - File Entries

0x000BE800 - First File Data

0x494A7800 - AnchorVolumeDescriptorPointer</strong>
