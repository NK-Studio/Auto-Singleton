# USingleton
싱글턴 패턴에 대하여 자동 기능이 탑재된 패키지입니다.
# 소개
USingleton은 유니티 개발에서 자주 사용하는 **Singleton 패턴을 빠르게 빌드업할 수 있도록 도와주는 패키지**입니다.

이 패키지는 **두가지 스타일**의 Singleton 방식을 지원합니다.
- Auto Singleton
- Self Singleton

### Auto Singleton
Auto Singleton은 클래스에 `Singleton` 어트리뷰트를 부여하면 자동으로 싱글턴 패턴을 적용해주는 기능입니다.  
또한, **런타임시 자동으로 생성되는 기능을 지원**합니다.

### Self Singleton
Self Singleton은 `Singleton` 클래스를 상속하여 싱글턴 패턴을 적용해주는 기능입니다.  
일반적으로 **런타임시 자동으로 생성되지 않습니다.** 만약, **해당 Singleton 인스턴스가 요구될 경우 Resources 폴더에서 자동 생성되는 처리가 동작**합니다.

# Auto Singleton VS Self Singleton
| | Auto Singleton | Self Singleton |                                          비고                                          |
|:---:|:---:|:---:|:------------------------------------------------------------------------------------:|
| 런타임시 자동 생성 | O | △ |                   Self Singleton은 인스턴스 요구시,<br/>Resources에서 로드되어 생성됩니다.                   |
| Resources 폴더에서 로드 | △ | O | Auto Singleton은 Addressable Asset System을 제공하여 Resource폴더가 아니더라도 동작시킬 수 있습니다. |
| DontDestroyOnLoad 재정의 | X | O |                                                                                      |

> [!NOTE]
> Auto Singleton은 어플리케이션이 실행되는 동안 계속 살아있는 싱글턴 객체를 만들고 싶을 때 사용하는 패턴입니다.
> 
> Self Singleton은 유동적으로 동작할 수 있으며, DontDestroyOnLoad를 재정의하여 씬이 변경되면 파괴되도록 할 수 있습니다.
> 
> 일반적으로는 Auto Singleton을 사용하며, DontDestroyOnLoad를 재정의하여 씬이 변경되면 파괴되도록 하고 싶다면 Self Singleton을 사용하는 것을 추천합니다.
