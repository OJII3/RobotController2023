# Robot Controller

ロボットの操縦・遠隔パラメータ確認に用いる Android アプリケーション(外部ゲーミングコントローラーを接続).

UDP 通信を用い、マルチキャストして LAN 内の UDP クライアントと接続(IP 取得)、ユニキャストにより通信する(対応した UDP クライアントが別途必要です).

また、通信の形式はバイト列で、通信相手側のデータ構造に合わせてデコーディングなどを変更する必要があります.

## Environment

OS: Arch Linux
Unity Editor: 2022.3.7f1 (3D, Legacy RP)
IDE: JetBrains Rider 2023.2

## Notes on Development

- Assets/RobotController にプロジェクトのソースがあります.
- 通信相手は ROS2(C++)で作成した UDP クライアントです.
- 通信相手の ROS2 ノードは、別の ROS2 ノードを経由してマイコンに USB 接続されたシリアル通信でデータを送受信します.
- 上記は私一人の担当範囲内なので、密結合でも問題ありません().
