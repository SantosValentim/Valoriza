import 'package:flutter/material.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  String _nome = '';
  int _denuncias = 0;
  int _resolvidas = 0;
  int _trilhas = 0;
  int _indicadores = 0;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final user = await AuthService().getUser();
    _nome = user?['nomeSocial'] ??
        user?['nomeCompleto'] ??
        user?['email'] ??
        'Usuário';

    try {
      final api = ApiService();
      final dens = await api.getDenuncias();
      final tris = await api.getTrilhas();
      List<dynamic> inds = [];
      try {
        inds = await api.getIndicadores();
      } catch (_) {}

      int resolvidas = 0;
      for (final d in dens) {
        if (d is Map &&
            (d['status']?.toString().toLowerCase() == 'resolvida')) {
          resolvidas++;
        }
      }

      setState(() {
        _denuncias = dens.length;
        _resolvidas = resolvidas;
        _trilhas = tris.length;
        _indicadores = inds.length;
        _loading = false;
      });
    } catch (_) {
      setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Center(child: CircularProgressIndicator());
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              color: const Color(0xFF1a1a2e),
              borderRadius: BorderRadius.circular(16),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Olá, $_nome',
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 22,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  'Painel de Diversidade, Equidade e Inclusão',
                  style: TextStyle(color: Colors.white.withValues(alpha: 0.75)),
                ),
              ],
            ),
          ),
          const SizedBox(height: 20),
          const Text('Resumo', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
          const SizedBox(height: 12),
          GridView.count(
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            crossAxisCount: 2,
            mainAxisSpacing: 12,
            crossAxisSpacing: 12,
            childAspectRatio: 1.35,
            children: [
              _MetricCard(
                titulo: 'Denúncias',
                valor: '$_denuncias',
                cor: const Color(0xFF1a1a2e),
                icon: Icons.report_outlined,
              ),
              _MetricCard(
                titulo: 'Resolvidas',
                valor: '$_resolvidas',
                cor: Colors.green.shade700,
                icon: Icons.check_circle_outline,
              ),
              _MetricCard(
                titulo: 'Trilhas',
                valor: '$_trilhas',
                cor: Colors.teal.shade700,
                icon: Icons.school_outlined,
              ),
              _MetricCard(
                titulo: 'Indicadores',
                valor: '$_indicadores',
                cor: Colors.deepPurple.shade600,
                icon: Icons.bar_chart,
              ),
            ],
          ),
          const SizedBox(height: 20),
          const Text('Atalhos', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
          const SizedBox(height: 8),
          const Text(
            'Use a barra inferior para abrir Treinamentos, Denúncias e Indicadores.',
            style: TextStyle(color: Colors.grey, fontSize: 13),
          ),
        ],
      ),
    );
  }
}

class _MetricCard extends StatelessWidget {
  final String titulo;
  final String valor;
  final Color cor;
  final IconData icon;

  const _MetricCard({
    required this.titulo,
    required this.valor,
    required this.cor,
    required this.icon,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.06),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
        border: Border(left: BorderSide(color: cor, width: 4)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(icon, color: cor, size: 22),
          const SizedBox(height: 8),
          Text(valor, style: TextStyle(fontSize: 26, fontWeight: FontWeight.bold, color: cor)),
          Text(titulo, style: TextStyle(fontSize: 13, color: Colors.grey.shade600)),
        ],
      ),
    );
  }
}