import 'dart:convert';
import 'dart:io';
import 'package:shared_preferences/shared_preferences.dart';
import 'api_service.dart';

class SyncService {
  final ApiService _apiService = ApiService();
  static const String _cacheKey = 'fila_denuncias_offline';

  // Salva a denúncia localmente se o emulador/celular estiver sem internet
  Future<void> salvarOffline(Map<String, dynamic> denuncia) async {
    final prefs = await SharedPreferences.getInstance();
    List<String> fila = prefs.getStringList(_cacheKey) ?? [];
    fila.add(jsonEncode(denuncia));
    await prefs.setStringList(_cacheKey, fila);
  }

  // Envia os registros salvos offline automaticamente para a API .NET
  Future<int> sincronizarDenuncias() async {
    final prefs = await SharedPreferences.getInstance();
    List<String> fila = prefs.getStringList(_cacheKey) ?? [];

    if (fila.isEmpty) return 0;

    List<String> itensSucesso = [];
    int totalSincronizado = 0;

    for (String jsonItem in fila) {
      try {
        final dados = jsonDecode(jsonItem);
        
        // Dispara usando o ApiService que você já tem no projeto
        await _apiService.enviarDenuncia(
          tipo: dados['tipo'],
          relato: dados['relato'],
          anonima: dados['anonima'],
        );

        itensSucesso.add(jsonItem);
        totalSincronizado++;
      } on SocketException {
        // Sem internet ainda: interrompe o loop e mantém o resto na fila
        break; 
      } catch (e) {
        // Se for um erro definitivo (ex: erro de validação do backend), remove da fila
        itensSucesso.add(jsonItem);
        print('Erro ao sincronizar item da fila: $e');
      }
    }

    // Remove do dispositivo apenas o que foi salvo com sucesso no SQL Server
    fila.removeWhere((item) => itensSucesso.contains(item));
    await prefs.setStringList(_cacheKey, fila);

    return totalSincronizado;
  }
}
