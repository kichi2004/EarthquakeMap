# EarthquakeMapについて
地震情報や緊急地震速報を取得して地図を描画するソフトウェアです。
[こちら](https://software.kichi2004.jp/2018/05/21/eqmap/)で配布しています。


# ライセンス
EarthquakeMap ソースコードはMITライセンスに基づき公開しています。


# EarthquakeMap のソースコードを利用される方へ
このリポジトリをクローン して Visual Studio などで開くと，依存関係等の問題が発生します．  
今後，単独で利用できるように改修を進めたいと考えていますが，現時点での修正方法等を以下に記しています．
- ソリューションにプロジェクト「ForRelease」が含まれていますが，EarthquakeMap からは利用していないため，ソリューションから削除してください．
- 以下の各依存関係を追加してください．
  - [EarthquakeLibrary](https://github.com/kichi2004/EarthuquakeLibrary)
  - [KyoshinMonitorLib 0.4.0](https://github.com/ingen084/KyoshinMonitorLib/releases/tag/v0.4.0.0)（NuGet でも入手可能）

【MITライセンスについて】

**許可されること**
- 商用利用
- 編集
- 再配布
- 私的利用

**制限事項**
- 免責
- 保証
(このソースコードを使用して何らかの問題が発生しても作者は一切責任を負いません。) 

**条件**
- ライセンス及び著作権表示

