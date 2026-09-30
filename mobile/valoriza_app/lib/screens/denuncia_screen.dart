/* Denúncias – lista + criar com Offline-First (SyncService) */

import 'package:flutter/material.dart';
import '../services/api_service.dart';
import '../services/sync_service.dart';

class DenunciasScreen extends StatefulWidget {
  const DenunciasScreen({super.key});

  @override
  State<DenunciasScreen> createState() => _DenunciasScreenState();
}

class _DenunciasScreenState extends State<DenunciasScreen> {
  final _api = ApiService();
  final _sync = SyncService();

  List<dynamic> _lista = [];
  bool _loading = true;
  int _filaOffline = 0;

  @override
  void initState() {
    super.initState();
    _iniciar();
  }

  /// Sincroniza fila offline e carrega lista da API
  Future<void> _iniciar() async {
    setState(() => _loading = true);
    try {
      final n = await _sync.sincronizarDenuncias();
      if (n > 0 && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('$n denúncia(s) offline enviada(s).')),
        );
      }
    } catch (_) {}
    await _carregar();
  }

  Future<void> _carregar() async {
    setState(() => _loading = true);
    try {
      _lista = await _api.getDenuncias();
    } catch (_) {
      _lista = [];
    }
    _filaOffline = await _sync.quantidadeNaFila();
    if (mounted) setState(() => _loading = false);
  }

  Color _corStatus(String? s) {
    switch ((s ?? '').toLowerCase()) {
      case 'resolvida':
        return Colors.green;
      case 'em análise':
      case 'emanalise':
      case 'em investigação':
        return Colors.orange;
      case 'arquivada':
        return Colors.grey;
      default:
        return const Color(0xFF1a1a2e);
    }
  }

  Future<void> _nova() async {
    final titulo = TextEditingController();
    final desc = TextEditingController();
    String? categoria;
    var anonima = false;

    final ok = await showModalBottomSheet<bool>(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
      ),
      builder: (ctx) {
        return Padding(
          padding: EdgeInsets.only(
            left: 20,
            right: 20,
            top: 20,
            bottom: MediaQuery.of(ctx).viewInsets.bottom + 20,
          ),
          child: StatefulBuilder(
            builder: (ctx, setLocal) => SingleChildScrollView(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  const Text(
                    'Nova denúncia',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 16),
                  TextField(
                    controller: titulo,
                    decoration: const InputDecoration(
                      labelText: 'Título',
                      border: OutlineInputBorder(),
                    ),
                  ),
                  const SizedBox(height: 12),
                  TextField(
                    controller: desc,
                    maxLines: 4,
                    decoration: const InputDecoration(
                      labelText: 'Descrição (mín. 20 caracteres)',
                      border: OutlineInputBorder(),
                      alignLabelWithHint: true,
                    ),
                  ),
                  const SizedBox(height: 12),
                  DropdownButtonFormField<String>(
                    // ignore: deprecated_member_use
                    value: categoria,
                    decoration: const InputDecoration(
                      labelText: 'Categoria',
                      border: OutlineInputBorder(),
                    ),
                    items: const [
                      DropdownMenuItem(value: 'Discriminação', child: Text('Discriminação')),
                      DropdownMenuItem(value: 'Assédio', child: Text('Assédio')),
                      DropdownMenuItem(value: 'Racismo', child: Text('Racismo')),
                      DropdownMenuItem(value: 'Outro', child: Text('Outro')),
                    ],
                    onChanged: (v) => setLocal(() => categoria = v),
                  ),
                  CheckboxListTile(
                    contentPadding: EdgeInsets.zero,
                    value: anonima,
                    onChanged: (v) => setLocal(() => anonima = v ?? false),
                    title: const Text('Enviar como anônima'),
                    subtitle: const Text('Gestores não verão seu nome'),
                  ),
                  const SizedBox(height: 8),
                  FilledButton(
                    onPressed: () => Navigator.pop(ctx, true),
                    child: const Text('Enviar denúncia'),
                  ),
                  TextButton(
                    onPressed: () => Navigator.pop(ctx, false),
                    child: const Text('Cancelar'),
                  ),
                ],
              ),
            ),
          ),
        );
      },
    );

    if (ok != true) return;

    final texto = desc.text.trim();
    if (texto.length < 20) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('A descrição deve ter pelo menos 20 caracteres.')),
      );
      return;
    }

    // Offline-First: tenta API; se falhar rede, grava na fila local
    try {
      final r = await _sync.enviarDenunciaOfflineFirst(
        titulo: titulo.text.trim().isEmpty ? null : titulo.text.trim(),
        descricao: texto,
        categoria: categoria,
        anonima: anonima,
      );

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(r.mensagem)),
      );

      await _carregar();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Erro: $e')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Center(child: CircularProgressIndicator());
    }

    return Scaffold(
      backgroundColor: const Color(0xFFF5F6FA),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _nova,
        icon: const Icon(Icons.add),
        label: const Text('Nova'),
        backgroundColor: const Color(0xFF1a1a2e),
        foregroundColor: Colors.white,
      ),
      body: RefreshIndicator(
        onRefresh: _iniciar,
        child: Column(
          children: [
            if (_filaOffline > 0)
              MaterialBanner(
                content: Text(
                  '$_filaOffline denúncia(s) aguardando envio (offline).',
                ),
                leading: const Icon(Icons.cloud_off),
                actions: [
                  TextButton(
                    onPressed: _iniciar,
                    child: const Text('Sincronizar'),
                  ),
                ],
              ),
            Expanded(
              child: _lista.isEmpty
                  ? ListView(
                      physics: const AlwaysScrollableScrollPhysics(),
                      children: const [
                        SizedBox(height: 100),
                        Center(child: Text('Nenhuma denúncia encontrada')),
                      ],
                    )
                  : ListView.builder(
                      physics: const AlwaysScrollableScrollPhysics(),
                      padding: const EdgeInsets.fromLTRB(12, 12, 12, 88),
                      itemCount: _lista.length,
                      itemBuilder: (_, i) {
                        final d = Map<String, dynamic>.from(_lista[i] as Map);
                        final status = d['status']?.toString() ?? 'Aberta';
                        final cor = _corStatus(status);
                        return Card(
                          margin: const EdgeInsets.only(bottom: 10),
                          elevation: 0,
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                          child: Padding(
                            padding: const EdgeInsets.all(14),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Row(
                                  children: [
                                    Expanded(
                                      child: Text(
                                        d['titulo']?.toString() ?? 'Denúncia',
                                        style: const TextStyle(
                                          fontWeight: FontWeight.w600,
                                          fontSize: 15,
                                        ),
                                      ),
                                    ),
                                    Container(
                                      padding: const EdgeInsets.symmetric(
                                        horizontal: 10,
                                        vertical: 4,
                                      ),
                                      decoration: BoxDecoration(
                                        color: cor.withValues(alpha: 0.12),
                                        borderRadius: BorderRadius.circular(20),
                                      ),
                                      child: Text(
                                        status,
                                        style: TextStyle(
                                          color: cor,
                                          fontSize: 12,
                                          fontWeight: FontWeight.w600,
                                        ),
                                      ),
                                    ),
                                  ],
                                ),
                                const SizedBox(height: 6),
                                Text(
                                  'Protocolo: ${d['protocolo'] ?? '—'}',
                                  style: TextStyle(
                                    color: Colors.grey.shade600,
                                    fontSize: 13,
                                  ),
                                ),
                                if (d['categoria'] != null) ...[
                                  const SizedBox(height: 4),
                                  Text(
                                    d['categoria'].toString(),
                                    style: TextStyle(
                                      color: Colors.grey.shade500,
                                      fontSize: 12,
                                    ),
                                  ),
                                ],
                              ],
                            ),
                          ),
                        );
                      },
                    ),
            ),
          ],
        ),
      ),
    );
  }
}