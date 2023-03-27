# AspNetCore.Identity.MongoDB

A libary that provides MongoDB UserStore and RoleStore implementations for ASP.NET Identity Core.
It allows you to use MongoDB with ASP.NET Core Identity.


## Identity Entities

If you choose to use custom user and role entities be reminded that those entities must inherit from 
```MongoIdentityUser<TKey>``` and ```MongoIdentityRole<TKey>```, where ```TKey``` is the type of the
ID of the entity. The library supports either ```string``` or ```Guid``` type IDs. When ```string```
is used, the MongoDB native ```ObjectId``` will be used as database ID type.

The following example shows custom user and role entities:

```C#
public class ApplicationUser : MongoIdentityUser<string>
{
	public ApplicationUser()
	{
	}

	public ApplicationUser(string userName) 
		: base(userName)
	{
	}

	public string FirstName { get; set; }

	public string LastName { get; set; }
}

public class ApplicationRole : MongoIdentityRole<string>
{
	public ApplicationRole()
	{
	}

	public ApplicationRole(string roleName)
		: base(roleName)
	{
	}

	public string DisplayText { get; set; }
}
```

Using custom entity classes allows the developer to extend to store additional data besides the data needed
by the Identity system.

If no custom enities are needed, one can use the default implementations using ```string``` IDs:

- ```MongoIdentityUser```
- ```MongoIdentityRole```

## Usage

To use the MongoDB stores with ASP.NET Identity use the ```IdentityBuilder``` extension ```AddMongoDbStores```
and configure the ```IdentityMongoDbContext``` using the ```AddMongoDbContext``` extension. 

The stores support user/role and user-only configuration.

### Configure user/role mode

```C#
builder.Services
	.AddAuthentication(IdentityConstants.ApplicationScheme)
	.AddIdentityCookies();

builder.Services.AddMongoDbContext<IdentityMongoDbContext>(options =>
{
	options.ConnectionString = "mongodb://localhost:27017";
	options.DatabaseName = "identity";
})
.AddIdentityCore<MongoIdentityUser>()
.AddRoles<MongoIdentityRole>()
.AddDefaultTokenProviders()
.AddMongoDbStores<SampleContext>();
```

### Configure user-only mode

```C#
builder.Services
	.AddAuthentication(IdentityConstants.ApplicationScheme)
	.AddIdentityCookies();

builder.Services.AddMongoDbContext<IdentityMongoDbContext>(options =>
{
	options.ConnectionString = "mongodb://localhost:27017";
	options.DatabaseName = "identity";
})
.AddIdentityCore<MongoIdentityUser>()
.AddDefaultTokenProviders()
.AddMongoDbStores<SampleContext>();
```

## Using a custom context

To use a custom ```IdentityMongoDbContext``` just create a new class that inherits from 
```IdentityMongoDbContext```. Using this context one can change the used collection names.
The default names are ```AspNetUsers``` and ```AspNetRoles```. The sample application shows
how to change them into custom names.

## Initialize the MongoDB driver

After configuring the service of the system one has to initialize the MongoDB stores database
using the ````IApplicationBuilder``` extension ```InitializeMongoDbStores```. The configuration 
will ensure the needed conventions are setup for the MongoDB driver and will ensure that the 
collections are created with the needed indexes for the user and role entities.

```C#
await app.InitializeMongoDbStores();
```

## Data Protection support

If the Identity system was configured to opt-in to use the data protection API the MongoDB driver
will be automatically configured to protect/unprotect properties annotated with the ```[ProtectedPersonalData]```
attribute by adding a specialized string serializer. You will still need to implement the 
needed services and specialized user store that implements ```IProtectedUserStore<TUser>```.

