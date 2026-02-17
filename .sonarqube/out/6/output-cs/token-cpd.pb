Ép
\C:\Users\sebahattin\Desktop\Dengeage\3.Hafta\FilmKirala\FilmKirala\FilmKirala.Api\Program.cs
var 
builder 
= 
WebApplication 
. 
CreateBuilder *
(* +
args+ /
)/ 0
;0 1
var 
	mcOptions 
= (
MessagePackSerializerOptions ,
., -
Standard- 5
. 
WithResolver 
( 
CompositeResolver #
.# $
Create$ *
(* +
NativeGuidResolver 
. 
Instance #
,# $!
NativeDecimalResolver 
. 
Instance &
,& '
StandardResolver   
.   
Instance   !
)!! 
)!! 
;!! 
MessagePackSerializer"" 
."" 
DefaultOptions"" $
=""% &
	mcOptions""' 0
;""0 1
builder%% 
.%% 
Logging%% 
.%% 
ClearProviders%% 
(%% 
)%%  
;%%  !
Log&& 
.&& 
Logger&& 

=&& 
new&& 
LoggerConfiguration&& $
(&&$ %
)&&% &
.'' 
MinimumLevel'' 
.'' 
Information'' 
('' 
)'' 
.(( 
Enrich(( 
.(( 
FromLogContext(( 
((( 
)(( 
.)) 
Enrich)) 
.)) 
WithProperty)) 
()) 
$str)) &
,))& '
$str))( 8
)))8 9
.** 
WriteTo** 
.** 
Console** 
(** 
)** 
.++ 
WriteTo++ 
.++ 
Graylog++ 
(++ 
new++ 
GraylogSinkOptions++ +
{,, 
HostnameOrAddress-- 
=-- 
$str-- '
,--' (
Port.. 
=.. 
$num.. 
,.. 
TransportType// 
=// 
Serilog// 
.//  
Sinks//  %
.//% &
Graylog//& -
.//- .
Core//. 2
.//2 3
	Transport//3 <
.//< =
TransportType//= J
.//J K
Udp//K N
}00 
)00 
.11 
CreateLogger11 
(11 
)11 
;11 
builder22 
.22 
Host22 
.22 

UseSerilog22 
(22 
)22 
;22 
var55 
configuration55 
=55 
builder55 
.55 
Configuration55 )
;55) *
builder:: 
.:: 
Services:: 
.:: &
AddStackExchangeRedisCache:: +
(::+ ,
options::, 3
=>::4 6
{;; 
options<< 
.<< 
Configuration<< 
=<< 
$str<< f
;<<f g
options== 
.== 
InstanceName== 
=== 
$str== (
;==( )
}>> 
)>> 
;>> 
builder@@ 
.@@ 
Services@@ 
.@@ 
	Configure@@ 
<@@ 
ApiBehaviorOptions@@ -
>@@- .
(@@. /
options@@/ 6
=>@@7 9
{AA 
optionsBB 
.BB +
SuppressModelStateInvalidFilterBB +
=BB, -
trueBB. 2
;BB2 3
}CC 
)CC 
;CC 
builderEE 
.EE 
ServicesEE 
.EE 
AddControllersEE 
(EE  
optionsEE  '
=>EE( *
{FF 
optionsGG 
.GG 
FiltersGG 
.GG 
AddGG 
<GG 
ValidationFilterGG (
>GG( )
(GG) *
)GG* +
;GG+ ,
}HH 
)HH 
;HH 
builderKK 
.KK 
ServicesKK 
.KK 
AddDbContextKK 
<KK 
AppDbContextKK *
>KK* +
(KK+ ,
optionsKK, 3
=>KK4 6
optionsLL 
.LL 
UseSqlServerLL 
(LL 
configurationLL &
.LL& '
GetConnectionStringLL' :
(LL: ;
$strLL; N
)LLN O
)LLO P
)LLP Q
;LLQ R
builderOO 
.OO 
ServicesOO 
.OO 
	AddScopedOO 
(OO 
typeofOO !
(OO! "
IGenericRepositoryOO" 4
<OO4 5
>OO5 6
)OO6 7
,OO7 8
typeofOO9 ?
(OO? @
GenericRepositoryOO@ Q
<OOQ R
>OOR S
)OOS T
)OOT U
;OOU V
builderPP 
.PP 
ServicesPP 
.PP 
	AddScopedPP 
<PP 
IMovieRepositoryPP +
,PP+ ,
MovieRepositoryPP- <
>PP< =
(PP= >
)PP> ?
;PP? @
builderQQ 
.QQ 
ServicesQQ 
.QQ 
	AddScopedQQ 
<QQ 
IUserRepositoryQQ *
,QQ* +
UserRepositoryQQ, :
>QQ: ;
(QQ; <
)QQ< =
;QQ= >
builderRR 
.RR 
ServicesRR 
.RR 
	AddScopedRR 
<RR 
IUnitOfWorkRR &
,RR& '

UnitOfWorkRR( 2
>RR2 3
(RR3 4
)RR4 5
;RR5 6
builderUU 
.UU 
ServicesUU 
.UU 
	AddScopedUU 
<UU 
IAuthServiceUU '
,UU' (
AuthServiceUU) 4
>UU4 5
(UU5 6
)UU6 7
;UU7 8
builderVV 
.VV 
ServicesVV 
.VV 
	AddScopedVV 
<VV 
IMovieServiceVV (
,VV( )
MovieServiceVV* 6
>VV6 7
(VV7 8
)VV8 9
;VV9 :
builderWW 
.WW 
ServicesWW 
.WW 
	AddScopedWW 
<WW 
IRentalServiceWW )
,WW) *
RentalServiceWW+ 8
>WW8 9
(WW9 :
)WW: ;
;WW; <
builderXX 
.XX 
ServicesXX 
.XX 
	AddScopedXX 
<XX 
IReviewServiceXX )
,XX) *
ReviewServiceXX+ 8
>XX8 9
(XX9 :
)XX: ;
;XX; <
builderYY 
.YY 
ServicesYY 
.YY 
	AddScopedYY 
<YY 
ICacheServiceYY (
,YY( )
CacheServiceYY* 6
>YY6 7
(YY7 8
)YY8 9
;YY9 :
builderZZ 
.ZZ 
ServicesZZ 
.ZZ 
	AddScopedZZ 
<ZZ 
IBusServiceZZ &
,ZZ& '

BusServiceZZ( 2
>ZZ2 3
(ZZ3 4
)ZZ4 5
;ZZ5 6
builder\\ 
.\\ 
Services\\ 
.\\ 
AddAutoMapper\\ 
(\\ 
typeof\\ %
(\\% &
MappingProfile\\& 4
)\\4 5
.\\5 6
Assembly\\6 >
)\\> ?
;\\? @
builder]] 
.]] 
Services]] 
.]] -
!AddFluentValidationAutoValidation]] 2
(]]2 3
)]]3 4
;]]4 5
builder^^ 
.^^ 
Services^^ 
.^^ /
#AddValidatorsFromAssemblyContaining^^ 4
<^^4 5
MappingProfile^^5 C
>^^C D
(^^D E
)^^E F
;^^F G
builderaa 
.aa 
Servicesaa 
.aa 
AddMassTransitaa 
(aa  
xaa  !
=>aa" $
{bb 
xcc 
.cc 
UsingRabbitMqcc 
(cc 
(cc 
contextcc 
,cc 
cfgcc !
)cc! "
=>cc# %
{dd 
cfgee 
.ee 
Hostee 
(ee 
$stree 
,ee 
$stree !
,ee! "
hee# $
=>ee% '
{ff 	
hgg 
.gg 
Usernamegg 
(gg 
$strgg 
)gg 
;gg  
hhh 
.hh 
Passwordhh 
(hh 
$strhh 
)hh 
;hh  
}ii 	
)ii	 

;ii
 
}jj 
)jj 
;jj 
}kk 
)kk 
;kk 
varnn 

jwtSectionnn 
=nn 
configurationnn 
.nn 

GetSectionnn )
(nn) *
$strnn* 7
)nn7 8
;nn8 9
varoo 
jwtKeyoo 

=oo 

jwtSectionoo 
[oo 
$stroo 
]oo 
??oo !
throwoo" '
newoo( +
	Exceptionoo, 5
(oo5 6
$stroo6 R
)ooR S
;ooS T
builderqq 
.qq 
Servicesqq 
.qq 
AddAuthenticationqq "
(qq" #
JwtBearerDefaultsqq# 4
.qq4 5 
AuthenticationSchemeqq5 I
)qqI J
.rr 
AddJwtBearerrr 
(rr 
optionsrr 
=>rr 
{ss 
optionstt 
.tt %
TokenValidationParameterstt )
=tt* +
newtt, /%
TokenValidationParameterstt0 I
{uu 	
ValidateIssuervv 
=vv 
truevv !
,vv! "
ValidateAudienceww 
=ww 
trueww #
,ww# $
ValidateLifetimexx 
=xx 
truexx #
,xx# $$
ValidateIssuerSigningKeyyy $
=yy% &
trueyy' +
,yy+ ,
ValidIssuerzz 
=zz 

jwtSectionzz $
[zz$ %
$strzz% -
]zz- .
,zz. /
ValidAudience{{ 
={{ 

jwtSection{{ &
[{{& '
$str{{' 1
]{{1 2
,{{2 3
IssuerSigningKey|| 
=|| 
new|| " 
SymmetricSecurityKey||# 7
(||7 8
Encoding||8 @
.||@ A
UTF8||A E
.||E F
GetBytes||F N
(||N O
jwtKey||O U
)||U V
)||V W
}}} 	
;}}	 

}~~ 
)~~ 
;~~ 
builderÄÄ 
.
ÄÄ 
Services
ÄÄ 
.
ÄÄ 
AddAuthorization
ÄÄ !
(
ÄÄ! "
)
ÄÄ" #
;
ÄÄ# $
builderÅÅ 
.
ÅÅ 
Services
ÅÅ 
.
ÅÅ %
AddEndpointsApiExplorer
ÅÅ (
(
ÅÅ( )
)
ÅÅ) *
;
ÅÅ* +
builderÑÑ 
.
ÑÑ 
Services
ÑÑ 
.
ÑÑ 
AddSwaggerGen
ÑÑ 
(
ÑÑ 
options
ÑÑ &
=>
ÑÑ' )
{ÖÖ 
options
ÜÜ 
.
ÜÜ 

SwaggerDoc
ÜÜ 
(
ÜÜ 
$str
ÜÜ 
,
ÜÜ 
new
ÜÜ  
OpenApiInfo
ÜÜ! ,
{
ÜÜ- .
Title
ÜÜ/ 4
=
ÜÜ5 6
$str
ÜÜ7 G
,
ÜÜG H
Version
ÜÜI P
=
ÜÜQ R
$str
ÜÜS W
}
ÜÜX Y
)
ÜÜY Z
;
ÜÜZ [
options
áá 
.
áá #
AddSecurityDefinition
áá !
(
áá! "
$str
áá" *
,
áá* +
new
áá, /#
OpenApiSecurityScheme
áá0 E
{
àà 
Name
ââ 
=
ââ 
$str
ââ 
,
ââ 
Type
ää 
=
ää  
SecuritySchemeType
ää !
.
ää! "
Http
ää" &
,
ää& '
Scheme
ãã 
=
ãã 
$str
ãã 
,
ãã 
BearerFormat
åå 
=
åå 
$str
åå 
,
åå 
In
çç 

=
çç 
ParameterLocation
çç 
.
çç 
Header
çç %
,
çç% &
Description
éé 
=
éé 
$str
éé &
}
èè 
)
èè 
;
èè 
options
êê 
.
êê $
AddSecurityRequirement
êê "
(
êê" #
new
êê# &(
OpenApiSecurityRequirement
êê' A
{
êêB C
{
ëë 	
new
íí #
OpenApiSecurityScheme
íí %
{
ìì 
	Reference
îî 
=
îî 
new
îî 
OpenApiReference
îî  0
{
îî1 2
Type
îî3 7
=
îî8 9
ReferenceType
îî: G
.
îîG H
SecurityScheme
îîH V
,
îîV W
Id
îîX Z
=
îî[ \
$str
îî] e
}
îîf g
}
ïï 
,
ïï 
Array
ññ 
.
ññ 
Empty
ññ 
<
ññ 
string
ññ 
>
ññ 
(
ññ  
)
ññ  !
}
óó 	
}
òò 
)
òò 
;
òò 
}ôô 
)
ôô 
;
ôô 
varúú 
app
úú 
=
úú 	
builder
úú
 
.
úú 
Build
úú 
(
úú 
)
úú 
;
úú 
appüü 
.
üü &
UseSerilogRequestLogging
üü 
(
üü 
)
üü 
;
üü 
app†† 
.
†† 
UseMiddleware
†† 
<
†† '
GlobalExceptionMiddleware
†† +
>
††+ ,
(
††, -
)
††- .
;
††. /
if¢¢ 
(
¢¢ 
app
¢¢ 
.
¢¢ 
Environment
¢¢ 
.
¢¢ 
IsDevelopment
¢¢ !
(
¢¢! "
)
¢¢" #
)
¢¢# $
{££ 
app
§§ 
.
§§ 

UseSwagger
§§ 
(
§§ 
)
§§ 
;
§§ 
app
•• 
.
•• 
UseSwaggerUI
•• 
(
•• 
)
•• 
;
•• 
}¶¶ 
appßß 
.
ßß !
UseHttpsRedirection
ßß 
(
ßß 
)
ßß 
;
ßß 
app®® 
.
®® 
UseAuthentication
®® 
(
®® 
)
®® 
;
®® 
app©© 
.
©© 
UseAuthorization
©© 
(
©© 
)
©© 
;
©© 
app™™ 
.
™™ 
MapControllers
™™ 
(
™™ 
)
™™ 
;
™™ 
await¨¨ 
app
¨¨ 	
.
¨¨	 

RunAsync
¨¨
 
(
¨¨ 
)
¨¨ 
;
¨¨ ø
zC:\Users\sebahattin\Desktop\Dengeage\3.Hafta\FilmKirala\FilmKirala\FilmKirala.Api\Middlewares\GlobalExceptionMiddleware.cs
	namespace 	

FilmKirala
 
. 
Api 
. 
Middlewares $
{ 
public 

class %
GlobalExceptionMiddleware *
{ 
private		 
readonly		 
RequestDelegate		 (
_next		) .
;		. /
public %
GlobalExceptionMiddleware (
(( )
RequestDelegate) 8
next9 =
)= >
{ 	
_next 
= 
next 
; 
} 	
public 
async 
Task 
InvokeAsync %
(% &
HttpContext& 1
context2 9
)9 :
{ 	
try 
{ 
await 
_next 
( 
context #
)# $
;$ %
} 
catch 
( 
	Exception 
ex 
)  
{ 
Log 
. 
Error 
( 
ex 
, 
$str :
,: ;
ex< >
.> ?
Message? F
)F G
;G H
var 

statusCode 
=  
ex! #
switch$ *
{  
KeyNotFoundException (
=>) +
(, -
int- 0
)0 1
HttpStatusCode1 ?
.? @
NotFound@ H
,H I'
UnauthorizedAccessException /
=>0 2
(3 4
int4 7
)7 8
HttpStatusCode8 F
.F G
UnauthorizedG S
,S T%
InvalidOperationException -
=>. 0
(1 2
int2 5
)5 6
HttpStatusCode6 D
.D E

BadRequestE O
,O P
_ 
=> 
( 
int 
) 
HttpStatusCode ,
., -
InternalServerError- @
}   
;   
await""  
HandleExceptionAsync"" *
(""* +
context""+ 2
,""2 3
ex""4 6
,""6 7

statusCode""8 B
)""B C
;""C D
}## 
}$$ 	
private%% 
static%% 
Task%%  
HandleExceptionAsync%% 0
(%%0 1
HttpContext%%1 <
context%%= D
,%%D E
	Exception%%F O
	exception%%P Y
,%%Y Z
int%%[ ^

statusCode%%_ i
)%%i j
{&& 	
context'' 
.'' 
Response'' 
.'' 
ContentType'' (
='') *
$str''+ =
;''= >
context(( 
.(( 
Response(( 
.(( 

StatusCode(( '
=((( )

statusCode((* 4
;((4 5
var** 
response** 
=** 
new** 
{++ 

StatusCode,, 
=,, 

statusCode,, '
,,,' (
Message-- 
=-- 
	exception-- #
.--# $
Message--$ +
,--+ ,
}// 
;// 
return00 
context00 
.00 
Response00 #
.00# $
WriteAsJsonAsync00$ 4
(004 5
response005 =
)00= >
;00> ?
}11 	
}22 
}33 ô
mC:\Users\sebahattin\Desktop\Dengeage\3.Hafta\FilmKirala\FilmKirala\FilmKirala.Api\Filters\ValidationFilter.cs
	namespace 	

FilmKirala
 
. 
Api 
. 
Filters  
{ 
public 

class 
ValidationFilter !
:" #
IActionFilter$ 1
{ 
public 
void 
OnActionExecuting %
(% &"
ActionExecutingContext& <
context= D
)D E
{		 	
if

 
(

 
!

 
context

 
.

 

ModelState

 #
.

# $
IsValid

$ +
)

+ ,
{ 
var 
errors 
= 
context $
.$ %

ModelState% /
./ 0
Values0 6
. 

SelectMany 
(  
v  !
=>" $
v% &
.& '
Errors' -
)- .
. 
Select 
( 
e 
=>  
e! "
." #
ErrorMessage# /
)/ 0
. 
ToList 
( 
) 
; 
context 
. 
Result 
=  
new! $"
BadRequestObjectResult% ;
(; <
new< ?
{ 
Message 
= 
$str 9
,9 :
Errors 
= 
errors #
} 
) 
; 
} 
} 	
public 
void 
OnActionExecuted $
($ %!
ActionExecutedContext% :
context; B
)B C
{ 	
} 	
} 
} ‰
qC:\Users\sebahattin\Desktop\Dengeage\3.Hafta\FilmKirala\FilmKirala\FilmKirala.Api\Controller\ReviewsController.cs
	namespace 	

FilmKirala
 
. 
Api 
. 
Controllers $
{ 
[		 
Route		 

(		
 
$str		 
)		 
]		 
[

 
ApiController

 
]

 
[ 
	Authorize 
] 
public 

class 
ReviewsController "
:# $
ControllerBase% 3
{ 
private 
readonly 
IReviewService '
_reviewService( 6
;6 7
public 
ReviewsController  
(  !
IReviewService! /
reviewService0 =
)= >
{ 	
_reviewService 
= 
reviewService *
;* +
} 	
[ 	
HttpPost	 
] 
public 
async 
Task 
< 
IActionResult '
>' (
	AddReview) 2
(2 3
[3 4
FromBody4 <
]< =
CreateReviewDto> M
requestN U
)U V
{ 	
var 
userIdString 
= 
User #
.# $
	FindFirst$ -
(- .

ClaimTypes. 8
.8 9
NameIdentifier9 G
)G H
?H I
.I J
ValueJ O
;O P
if 
( 
string 
. 
IsNullOrEmpty $
($ %
userIdString% 1
)1 2
)2 3
return4 :
Unauthorized; G
(G H
)H I
;I J
int 
userId 
= 
int 
. 
Parse "
(" #
userIdString# /
)/ 0
;0 1
try 
{ 
await 
_reviewService $
.$ %
AddReviewAsync% 3
(3 4
request4 ;
,; <
userId= C
)C D
;D E
return   
Ok   
(   
new   
{   
message    '
=  ( )
$str  * >
}  ? @
)  @ A
;  A B
}!! 
catch"" 
("" 
	Exception"" 
ex"" 
)""  
{## 
return$$ 

BadRequest$$ !
($$! "
new$$" %
{$$& '
error$$( -
=$$. /
ex$$0 2
.$$2 3
Message$$3 :
}$$; <
)$$< =
;$$= >
}%% 
}&& 	
}'' 
}(( º
qC:\Users\sebahattin\Desktop\Dengeage\3.Hafta\FilmKirala\FilmKirala\FilmKirala.Api\Controller\RentalsController.cs
	namespace 	

FilmKirala
 
. 
Api 
. 
Controllers $
{ 
[		 
ApiController		 
]		 
[

 
Route

 

(


 
$str

 
)

 
]

 
[ 
AllowAnonymous 
] 
public 

class 
RentalsController "
:# $
ControllerBase% 3
{ 
private 
readonly 
IRentalService '
_rentalService( 6
;6 7
public 
RentalsController  
(  !
IRentalService! /
rentalService0 =
)= >
=>? A
_rentalServiceB P
=Q R
rentalServiceS `
;` a
[ 	
HttpPost	 
( 
$str 
) 
] 
public 
async 
Task 
< 
IActionResult '
>' (
	RentMovie) 2
(2 3
[3 4
FromBody4 <
]< =
RentRequestDto> L
requestM T
)T U
{ 	
if 
( 
! 
int 
. 
TryParse 
( 
User "
." #
FindFirstValue# 1
(1 2

ClaimTypes2 <
.< =
NameIdentifier= K
)K L
,L M
outN Q
varR U
userIdV \
)\ ]
)] ^
{ 
userId 
= 
$num 
; 
} 
var 
result 
= 
await 
_rentalService -
.- .
RentMovieAsync. <
(< =
request= D
,D E
userIdF L
)L M
;M N
return 
Ok 
( 
result 
) 
; 
} 	
[!! 	
HttpGet!!	 
(!! 
$str!! 
)!! 
]!! 
public"" 
async"" 
Task"" 
<"" 
IActionResult"" '
>""' (
GetMyRentals"") 5
(""5 6
)""6 7
{## 	
if$$ 
($$ 
!$$ 
int$$ 
.$$ 
TryParse$$ 
($$ 
User$$ "
.$$" #
FindFirstValue$$# 1
($$1 2

ClaimTypes$$2 <
.$$< =
NameIdentifier$$= K
)$$K L
,$$L M
out$$N Q
var$$R U
userId$$V \
)$$\ ]
)$$] ^
{%% 
userId&& 
=&& 
$num&& 
;&& 
}'' 
var)) 
rentals)) 
=)) 
await)) 
_rentalService))  .
.)). /
GetUserRentalsAsync))/ B
())B C
userId))C I
)))I J
;))J K
return** 
Ok** 
(** 
rentals** 
)** 
;** 
}++ 	
},, 
}-- Œ
pC:\Users\sebahattin\Desktop\Dengeage\3.Hafta\FilmKirala\FilmKirala\FilmKirala.Api\Controller\MoviesController.cs
	namespace 	

FilmKirala
 
. 
Api 
. 
Controllers $
{ 
[ 
Route 

(
 
$str 
) 
] 
[		 
ApiController		 
]		 
public

 

class

 
MoviesController

 !
:

" #
ControllerBase

$ 2
{ 
private 
readonly 
IMovieService &
_movieService' 4
;4 5
public 
MoviesController 
(  
IMovieService  -
movieService. :
): ;
{ 	
_movieService 
= 
movieService (
;( )
} 	
[ 	
HttpGet	 
] 
public 
async 
Task 
< 
IActionResult '
>' (
GetAll) /
(/ 0
)0 1
{ 	
var 
result 
= 
await 
_movieService ,
., -
GetAllMoviesAsync- >
(> ?
)? @
;@ A
return 
Ok 
( 
result 
) 
; 
} 	
[ 	
HttpGet	 
( 
$str 
) 
] 
public 
async 
Task 
< 
IActionResult '
>' (
GetById) 0
(0 1
int1 4
id5 7
)7 8
{ 	
var 
result 
= 
await 
_movieService ,
., -
GetMovieByIdAsync- >
(> ?
id? A
)A B
;B C
return 
Ok 
( 
result 
) 
; 
} 	
[!! 	
	Authorize!!	 
(!! 
Roles!! 
=!! 
$str!! !
)!!! "
]!!" #
["" 	
HttpPost""	 
]"" 
public## 
async## 
Task## 
<## 
IActionResult## '
>##' (
Add##) ,
(##, -
[##- .
FromBody##. 6
]##6 7
CreateMovieDto##8 F
request##G N
)##N O
{$$ 	
await%% 
_movieService%% 
.%%  
AddMovieAsync%%  -
(%%- .
request%%. 5
)%%5 6
;%%6 7
return&& 
Ok&& 
(&& 
new&& 
{&& 
message&& #
=&&$ %
$str&&& ?
}&&@ A
)&&A B
;&&B C
}'' 	
}(( 
})) ÷
nC:\Users\sebahattin\Desktop\Dengeage\3.Hafta\FilmKirala\FilmKirala\FilmKirala.Api\Controller\AuthController.cs
	namespace 	

FilmKirala
 
. 
Api 
. 
Controllers $
{ 
[ 
ApiController 
] 
[		 
Route		 

(		
 
$str		 
)		 
]		 
public

 

class

 
AuthController

 
:

  !
ControllerBase

" 0
{ 
private 
readonly 
IAuthService %
_authService& 2
;2 3
public 
AuthController 
( 
IAuthService *
authService+ 6
)6 7
=>8 :
_authService; G
=H I
authServiceJ U
;U V
[ 	
HttpPost	 
( 
$str 
) 
] 
public 
async 
Task 
< 
IActionResult '
>' (
Register) 1
(1 2
[2 3
FromBody3 ;
]; <
RegisterRequestDto= O
requestP W
)W X
{ 	
var 
result 
= 
await 
_authService +
.+ ,
RegisterAsync, 9
(9 :
request: A
)A B
;B C
return 
CreatedAtAction "
(" #
nameof# )
() *
Login* /
)/ 0
,0 1
new2 5
{6 7
email8 =
=> ?
request@ G
.G H
EmailH M
}N O
,O P
resultQ W
)W X
;X Y
} 	
[ 	
HttpPost	 
( 
$str 
) 
] 
public 
async 
Task 
< 
IActionResult '
>' (
Login) .
(. /
[/ 0
FromBody0 8
]8 9
LoginRequestDto: I
requestJ Q
)Q R
=> 
Ok 
( 
await 
_authService $
.$ %

LoginAsync% /
(/ 0
request0 7
)7 8
)8 9
;9 :
[ 	
HttpPost	 
( 
$str !
)! "
]" #
public 
async 
Task 
< 
IActionResult '
>' (
RefreshToken) 5
(5 6
[6 7
FromBody7 ?
]? @"
RefreshTokenRequestDtoA W
requestX _
)_ `
=> 
Ok 
( 
await 
_authService $
.$ %
RefreshTokenAsync% 6
(6 7
request7 >
)> ?
)? @
;@ A
[ 	
	Authorize	 
( 
Roles 
= 
$str "
)" #
]# $
[   	
HttpPost  	 
(   
$str   "
)  " #
]  # $
public!! 
async!! 
Task!! 
<!! 
IActionResult!! '
>!!' (
UpdateBalance!!) 6
(!!6 7
[!!7 8
FromBody!!8 @
]!!@ A
UpdateBalanceDto!!B R
request!!S Z
)!!Z [
{"" 	
await$$ 
_authService$$ 
.$$ "
UpdateUserBalanceAsync$$ 5
($$5 6
request$$6 =
.$$= >
Email$$> C
,$$C D
($$E F
int$$F I
)$$I J
request$$J Q
.$$Q R

NewBalance$$R \
)$$\ ]
;$$] ^
return&& 
Ok&& 
(&& 
new&& 
{&& 
Message&& #
=&&$ %
$str&&& J
}&&K L
)&&L M
;&&M N
}'' 	
}(( 
})) 