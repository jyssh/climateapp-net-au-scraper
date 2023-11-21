import scala.sys.process._

val ids = List(
  "b779e173-4929-4c2e-ac29-66babf17064b",
  "aa71e4de-4251-4af1-9bcb-a6f9bda50fb5",
  "41e009da-c784-432b-9371-5061c318b8aa",
  "bc4f3fe2-62e1-4760-ad06-75a996a85999",
  "d1caa6ae-6c50-41d8-902f-138784ca3afd",
  "3e741098-1b67-4fa4-b471-3d9d4b94fc04",
  "c06a3cad-7ccb-4860-a2ad-1d989058dfe8",
  "eaf4d133-2b2c-41e9-b63b-9a98b386a8c9",
  "60caa58f-da65-44c5-8593-74e5ae21163e",
  "ad68a863-e4b4-47cc-ad97-f546ff5238f1",
  "652dbc71-c9ba-4835-a76e-989af53d129c",
  "c0c2349f-7e31-484a-a7b1-dc119abbaab0",
  "9448da8b-c381-437a-8aaa-b2d2c4b1a7ca",
  "ff520441-551c-4b20-a945-55271e381ac9",
  "6bc12f31-357a-424b-b879-07f46ba40e86",
  "46b60201-bd21-4470-a24e-647b545b5cfa",
  "821a991e-a7b2-46d6-926c-f64ee4e5e517"
)

@main def main() =
  ids.foreach(id => {
    val url = s"https://climateapp.net.au/Library/Details/$id"
    val cmd = s"wget --load-cookies=cookies.txt $url"
    val _ = cmd.!!
  })
