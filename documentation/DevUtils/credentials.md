# Staff credentials

A collection of users which can be used for testing endpoints and features.

### Owners
These should be available only to us as owners of SAAS, and in general aren't meant to be used in frontend at all.
1. username: testowner      password: test

### Admin
Admin is a "superuser" of manager, i.e. has access to all places owned by 1 business.
Currently, this one is unimportant as we do not support any features for multiple places at once.
1. username: testadmin      password: test
2. username: vivasadmin     password: test123

### Managers
Manager is superuser within single place.
Should be used for desktop-login as it can access dashboard for adding tables, employees, menus. itd
Best would be to use vivasmanager, as data is populated for this place.

1. username: vivasmanager   password: test
2. username: testmanager    password: test123

### Regular
1. username: teststaff      password: 123456
2. username: vivasilica_reg password: test
3. username: vivas_trg      password: authtest

---
Any other user you want to test probably has the password test.
You can add employees to place by logging into manager account and using the POST /staff endpoint.
Set username, role and provide password.
