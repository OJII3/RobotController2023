# Robot Controller

ロボットの操縦・遠隔パラメータ確認に用いる Android アプリケーション(外部ゲーミングコントローラーを接続).

数ミリ秒おきに、UDP でブロードキャストを行い、ゲームパッドの入力状態を符号無しバイト列で送信します.

同時に、UDP でメッセージを受信し、画面にログを表示します.

## Environment

OS: Arch Linux
Unity Editor: 2022.3.7f1 (3D, Legacy RP)
IDE: JetBrains Rider 2023.2

## Stack

- Unity
- C#
- UniTask
- Input System

## Notes on Development

- Assets/RobotController にプロジェクトのソースがあります.
- 通信相手は ROS2(C++)で作成した UDP クライアントです.
- 通信相手の ROS2 ノードは、別の ROS2 ノードを経由してマイコンに USB 接続されたシリアル通信でデータを送受信します.
- 上記は私一人の担当範囲内なので、密結合でも問題ありません
