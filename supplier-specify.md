# SupplierService Specification - Permission-Based Authorization Migration

## Permissions to Define

### Supplier Operations
```
supplier.suppliers.create        - Create new suppliers
supplier.suppliers.read          - Read supplier details
supplier.suppliers.update        - Update supplier information
supplier.suppliers.delete        - Delete suppliers
supplier.suppliers.approve       - Approve new suppliers
supplier.suppliers.suspend       - Suspend suppliers
supplier.suppliers.export        - Export supplier data
```

### Contact Operations
```
supplier.contacts.create         - Create supplier contacts
supplier.contacts.read           - Read contact details
supplier.contacts.update         - Update contacts
supplier.contacts.delete         - Delete contacts
```

### Performance Operations
```
supplier.performance.view        - View supplier performance metrics
supplier.performance.rate        - Rate supplier performance
supplier.performance.report      - Generate performance reports
```

## Predefined Roles

### supplier-admin
**Permissions**: All supplier.* permissions

### supplier-manager
**Permissions**: create, read, update, approve, suspend, contacts.*, performance.*

### supplier-coordinator
**Permissions**: create, read, update, contacts.*, performance.view

### supplier-viewer
**Permissions**: read, contacts.read, performance.view

## Success Criteria
- [ ] ~14 permissions registered
- [ ] 4 predefined roles registered
