# ISAM Database Implementation

This project involves implementing the **Indexed Sequential Access Method (ISAM)** database structure, developed as part of the **Database Structures** course.

## Overview
The ISAM (Indexed Sequential Access Method) is a technique used to organize and access data efficiently in a file system. It combines the advantages of both **sequential access** and **indexed access**, offering a balance between fast record lookups and efficient data storage. This implementation includes:

- **Index area** for fast retrieval of records
- **Main data area** for primary data storage
- **Overflow area** to manage insertions beyond the main storage capacity
- **Configurable parameters** for flexibility and performance tuning

## Key Features

### **Core Operations:**
- **Insert**: Add new records to the database
- **Update**: Modify existing records
- **Delete**: Remove records from the database
- **Search**: Find records based on a key
- **Reorganize**: Restructure the database for better performance and space utilization

### **Buffer Management:**
- **Page buffering** for optimized I/O operations, improving access speed and minimizing disk operations

### **Configurable Parameters:**
- **Page size** (blocking factor) – Controls the size of each page in the database
- **Buffer size** – Determines the amount of memory allocated for buffering pages
- **Fill factor (α)** – Controls space utilization within each page to optimize insertion performance
- **Overflow area size factor (β)** – Defines the capacity of the overflow area to handle overflowed data
- **Reorganization threshold (γ)** – Specifies when the database should be reorganized for efficient storage and access
