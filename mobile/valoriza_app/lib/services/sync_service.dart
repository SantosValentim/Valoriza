/* Offline-First para denúncias
   1) Tenta enviar à API
   2) Se falhar (sem rede), grava na fila local
   3) Ao voltar a conexão, sincroniza a fila */

import 'dart:convert';
import 'dart:io';
import 'package:shared_preferences/shared_preferences.dart';
import 'api_service.dart';

class SyncService {
  final ApiService _apiService = ApiService();
  static const String _cacheKey = 'fila_denuncias_offline';

  /// Offline-First: tenta a API; se não houver rede, salva localmente.
  /// Retorna `enviado` = true se foi para a API agora ou `enviado` = false se ficou na fila offline
  Future<({bool enviado, String mensagem})> enviarDenunciaOfflineFirst({
    String? titulo,
    required String descricao,
    String? categoria,
    required bool anonima,
  }) async {
    final payload = <String, dynamic>{
      'titulo': titulo,
      'descricao': descricao,
      'categoria': categoria,
      'anonima': anonima,
      'criadoEmLocal': DateTime.now().toIso8601String(),
    };

    try {
      await _apiService.enviarDenuncia(
        titulo: titulo,
        descricao: descricao,
        categoria: categoria,
        anonima: anonima,
      );
      return (enviado: true, mensagem: 'Denúncia enviada com sucesso.');
    } on SocketException {
      await _salvarOffline(payload);
      return (
        enviado: false,
        mensagem: 'Sem internet. Denúncia salva no aparelho e será enviada depois.',
      );
    } on HttpException {
      await _salvarOffline(payload);
      return (
        enviado: false,
        mensagem: 'Falha de rede. Denúncia salva offline.',
      );
    } catch (e) {
      // Timeout / client offline costuma cair aqui como Exception genérica
      final msg = e.toString().toLowerCase();
      if (msg.contains('socket') ||
          msg.contains('network') ||
          msg.contains('connection') ||
          msg.contains('failed host')) {
        await _salvarOffline(payload);
        return (
          enviado: false,
          mensagem: 'Sem conexão. Denúncia salva offline.',
        );
      }
      // Erro de validação/API: não coloca na fila
      rethrow;
    }
  }

  /// Salva na fila local (SharedPreferences)
  Future<void> _salvarOffline(Map<String, dynamic> denuncia) async {
    final prefs = await SharedPreferences.getInstance();
    final fila = prefs.getStringList(_cacheKey) ?? <String>[];
    fila.add(jsonEncode(denuncia));
    await prefs.setStringList(_cacheKey, fila);
  }

  /// Quantidade de denúncias ainda não sincronizadas
  Future<int> quantidadeNaFila() async {
    final prefs = await SharedPreferences.getInstance();
    return (prefs.getStringList(_cacheKey) ?? []).length;
  }

  /// Envia a fila offline para a API (chame no login, ao abrir Denúncias ou ao voltar online)
  Future<int> sincronizarDenuncias() async {
    final prefs = await SharedPreferences.getInstance();
    final fila = prefs.getStringList(_cacheKey) ?? <String>[];
    if (fila.isEmpty) return 0;

    final restantes = <String>[];
    var totalSincronizado = 0;

    for (final jsonItem in fila) {
      try {
        final dados = jsonDecode(jsonItem) as Map<String, dynamic>;

        await _apiService.enviarDenuncia(
          titulo: dados['titulo'] as String?,
          descricao: (dados['descricao'] as String?) ?? '',
          categoria: dados['categoria'] as String?,
          anonima: dados['anonima'] == true,
        );

        totalSincronizado++;
      } on SocketException {
        // Sem internet: mantém este e os próximos na fila
        restantes.add(jsonItem);
        // Inclui o restante sem tentar de novo agora
        final idx = fila.indexOf(jsonItem);
        if (idx >= 0 && idx < fila.length - 1) {
          restantes.addAll(fila.sublist(idx + 1));
        }
        break;
      } catch (e) {
        // Erro definitivo (validação etc.): descarta o item para não travar a fila
        print('Item removido da fila (erro definitivo): $e');
      }
    }

    await prefs.setStringList(_cacheKey, restantes);
    return totalSincronizado;
  }
}