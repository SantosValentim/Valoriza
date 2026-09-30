import 'package:flutter/material.dart';
import '../services/api_service.dart';

class IndicadoresScreen extends StatefulWidget {
  const IndicadoresScreen({super.key});

  @override
  State<IndicadoresScreen> createState() => _IndicadoresScreenState();
}

class _IndicadoresScreenState extends State<IndicadoresScreen> {
  List<dynamic> _lista = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      _lista = await ApiService().getIndicadores();
    } catch (_) {}
    setState(() => _loading = false);
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) return const Center(child: CircularProgressIndicator());

    if (_lista.isEmpty) {
      return const Center(child: Text('Nenhum indicador cadastrado'));
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView.builder(
        padding: const EdgeInsets.all(12),
        itemCount: _lista.length,
        itemBuilder: (_, i) {
          final ind = Map<String, dynamic>.from(_lista[i] as Map);
          final mes = ind['mes'];
          final ano = ind['ano'];
          final total = ind['totalColaboradores'] ?? 0;
          final negros = ind['colaboradoresNegros'] ?? 0;
          final indigenas = ind['colaboradoresIndigenas'] ?? 0;
          final adesao = ind['percentualAdesaoTreinamentos'] ?? 0;

          return Card(
            margin: const EdgeInsets.only(bottom: 12),
            elevation: 0,
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Período $mes/$ano',
                    style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 12),
                  Row(
                    children: [
                      _mini('Total', '$total'),
                      _mini('Negros', '$negros'),
                      _mini('Indígenas', '$indigenas'),
                      _mini('Adesão', '$adesao%'),
                    ],
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _mini(String label, String value) {
    return Expanded(
      child: Column(
        children: [
          Text(value, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
          Text(label, style: TextStyle(fontSize: 11, color: Colors.grey.shade600)),
        ],
      ),
    );
  }
}