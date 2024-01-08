# Messager
Messager 기능은 유니티의 SendMessage와 비슷한 기능을 제공합니다.  
다만, SendMessage는 계층 구조의 모든 오브젝트를 검사하는 반면, Messager는 관리되는 Dictionary에 등록한 메세지를 검사하여 성능 부분에서 월등합니다.

---
# 시작해요
## 전체 코드

예시를 위해 두 가지 클래스를 작성해보겠습니다.  
[!code-csharp[](../code/Messager/ScreenEffect.cs)]  
[!code-csharp[](../code/Messager/TestCode.cs)]
## 등록하기
[!code-csharp[](../code/Messager/Example/ScreenEffect01.cs?highlight=2,8-9)]
다음과 같이 작성하여 Awake에서 등록 작업을 수행합니다.
## 해제하기
[!code-csharp[](../code/Messager/Example/ScreenEffect02.cs?highlight=2,9,10)]  
다음과 같이 작성하여 OnDestroy에서 해제 작업을 수행합니다.
## 메세지 보내기
[!code-csharp[](../code/Messager/TestCode.cs?highlight=2,10,15)]  
다음과 같이 작성하여 메세지를 보낼 수 있습니다.